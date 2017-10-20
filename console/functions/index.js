/**
 * Copyright 2017 Google Inc. All Rights Reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for t`he specific language governing permissions and
 * limitations under the License.
 */
'use strict';

const functions = require('firebase-functions');
const admin = require('firebase-admin');
admin.initializeApp(functions.config().firebase);

// Database key for Leaderboard/Map/{MapType}/{MapID}/Top/lastCleanup
const DB_KEY_LAST_CLEANUP = "lastCleanup";

// Database key for Leaderboard/Map/{MapType}/{MapID}/Top/SharedReplays
const DB_KEY_SHARED_REPLAYS = "SharedReplays";

// Database key to specify if the replay record is shared in the rank record.
// Leaderboard/Map/{MapType}/{MapID}/Top/Ranks/{RankID}/isShared
const DB_KEY_IS_SHARED = "isShared";

// Database key to storage url of the replay data in the rank record. Ex.
// Leaderboard/Map/{MapType}/{MapID}/Top/Ranks/{RankID}/replayPath and
// Leaderboard/Map/{MapType}/{MapID}/Top/SharedReplay/{RankID}/replayPath
const DB_KEY_REPLAY_PATH = "replayPath";

// Database key of the time of this rank record. Ex.
// Leaderboard/Map/{MapType}/{MapID}/Top/Ranks/{RankID}/time and
// Leaderboard/Map/{MapType}/{MapID}/Top/SharedReplay/{RankID}/time
const DB_KEY_TIME = "time";

// Minimum time between two cleanup process in milliseconds (1 minutes)
// The cloud function aborts if it is triggered less than this amount of time since last cleanup.
const CLEANUP_MIN_INTV_IN_MS = 1000 * 60;

// Target number of records to keep under
// Leaderboard/Map/{MapType}/{MapID}/Top/Ranks/ and
// Leaderboard/Map/{MapType}/{MapID}/Top/SharedReplay/
const TARGET_MAX_RECORDS = 5;

// Start to clean up after the child count exceed this number.
// The cleanup process will keep only the top "TARGET_MAX_RECORDS" of record and remove the rest.
const RECORDS_CLEANUP_THRESHOLD = 10;

// This cloud function is to limit the number of children in under the following database url
//   Leaderboard/Map/{MapType}/{MapID}/Top/Ranks/
//   Leaderboard/Map/{MapType}/{MapID}/Top/SharedReplays/
// Also, this function removes the replay data from the cloud storage if it is no longer referenced
// by the database.
// This cloud function is designed to process clean-up in a batch for efficiency.
exports.processRankAdded =
    functions.database.ref('Leaderboard/Map/{MapType}/{MapID}/Top/Ranks/{RankID}')
    .onCreate(event => {
  console.log("========= Rank Record Added =========");

  return updateLastCleanupTimestamp(event).then(result => {
    // If the transaction is aborted because the last update timestamp is too closed to current
    // time, do nothing.  Otherwise, start the clean-up process.
    if (result.committed) {
      return processCleanup(event);
    } else {
      return console.log("Skip clean-up process");
    }
  });
});

// Reduce the freqency of the heavy lifting clean-up process through lastCleanup timestamp and
// predefined CLEANUP_MIN_INTV_IN_MS property.
function updateLastCleanupTimestamp(event) {
  const lastCleanupTimestampRef = event.data.adminRef.parent.parent.child(DB_KEY_LAST_CLEANUP);
  // Use transaction to update timestamp to prevent racing condition
  return lastCleanupTimestampRef.transaction(snapshot => {
    const timestamp = new Date().getTime();
    if (snapshot !== null && (timestamp - snapshot) <= CLEANUP_MIN_INTV_IN_MS) {
      // Abort the transaction by returning undefined
      return console.log("Abort " + lastCleanupTimestampRef + " update ( " + (timestamp - snapshot)
        + "ms <= " + CLEANUP_MIN_INTV_IN_MS + "ms )");
    } else {
      // Update the timestamp
      console.log("Update " + lastCleanupTimestampRef + " to " + timestamp);
      return timestamp;
    }
  });
}

// Limit the number of ranks records and shared replay records in the database and remove
// unreferenced replay files.
// This implementation assumes that a replay file is unreferenced if
// a) The rank record referencing it is removed and is marked as not shared
// b) The shared replay record referencing it is removed
// The assumption is based on that both rank record and shared replay record shares the same
// TARGET_MAX_RECORDS and only the reference in these records matter.
function processCleanup(event) {
  console.log("Starting clean-up process");

  const ranksRef = event.data.adminRef.parent;
  console.log("Create promises to clean up " + ranksRef);
  var promiseCleanupRanks =
      createCleanupTasks(ranksRef, TARGET_MAX_RECORDS, RECORDS_CLEANUP_THRESHOLD, rankRecord => {
    return rankRecord[DB_KEY_IS_SHARED] !== true;
  });

  const replayRef = event.data.adminRef.parent.parent.child(DB_KEY_SHARED_REPLAYS);
  console.log("Create promises to clean up " + replayRef);
  var promiseCleanupSharedReplays =
      createCleanupTasks(replayRef, TARGET_MAX_RECORDS, RECORDS_CLEANUP_THRESHOLD);

  return Promise.all([promiseCleanupRanks, promiseCleanupSharedReplays]).then(
      undefined, error => {
    console.log("processCleanup failed. ", error);
  });
}

// This function reduces number of child under dbRef to targetChildCount if the number exceeds
// cleanupThreshold.  It also attempt to remove the replay file reference by the child if
// fileRemovalFilterFunc() is not passed in or if it returns true.
function createCleanupTasks(dbRef, targetChildCount, cleanupThreshold, fileRemovalFilterFunc) {
  const bucket = admin.storage().bucket();

  return dbRef.orderByChild(DB_KEY_TIME).once('value').then(snapshot => {
    const childCount = snapshot.numChildren();
    if (childCount > cleanupThreshold) {
      const cleanupPromises = [];
      const recordToRemove = {};
      let childIndex = 0;
      // DataSnapshot.forEach preserves the ascending order by time from orderByChild(DB_KEY_TIME)
      snapshot.forEach(function(childSnapshot) {
        // Only keep the first few records
        if (childIndex >= targetChildCount) {
          // Set the child record to null to remove it in the database
          recordToRemove[childSnapshot.key] = null;

          const rankRecord = childSnapshot.val();
          const replayPath = rankRecord[DB_KEY_REPLAY_PATH];
          if (replayPath &&
              (typeof fileRemovalFilterFunc !== 'function' || fileRemovalFilterFunc(rankRecord))) {
            console.log("Removing file " + replayPath);
            // Add the promise to remove the replay record from the storage
            cleanupPromises.push(bucket.file(replayPath).delete());
          }
        }
        ++childIndex;
      });

      // Add the promise to remove extra childrens under dbRef
      cleanupPromises.push(dbRef.update(recordToRemove));

      return Promise.all(cleanupPromises).then(undefined, error =>{
        return console.log("createCleanupTasks for " + dbRef + " failed. ", error);
      });
    } else {
      return console.log("Not enough child to clean-up for " + dbRef
          + " ( " + childCount + " <= " + cleanupThreshold + " )");
    }
  });
}

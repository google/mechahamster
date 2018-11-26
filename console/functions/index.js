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

// Database key for Leaderboard/Map/{MapType}/{MapID}/Top/Ranks/lastCleanup
// and Leaderboard/Map/{MapType}/{MapID}/Top/SharedReplays/lastCleanup
const DB_KEY_LAST_CLEANUP = "lastCleanup";

// Database key for Leaderboard/Map/{MapType}/{MapID}/Top/SharedReplays
const DB_KEY_SHARED_REPLAYS = "SharedReplays";

// Database keys to storage url of the replay data in the rank record. Ex.
// Leaderboard/Map/{MapType}/{MapID}/Top/Ranks/{RankID}/data/replayPath and
// Leaderboard/Map/{MapType}/{MapID}/Top/SharedReplay/{RankID}/data/replayPath
const DB_KEY_DATA = "data";
const DB_KEY_REPLAY_PATH = "replayPath";

// Database key of the time of this rank record. Ex.
// Leaderboard/Map/{MapType}/{MapID}/Top/Ranks/{RankID}/score and
// Leaderboard/Map/{MapType}/{MapID}/Top/SharedReplay/{RankID}/score
const DB_KEY_SCORE = "score";

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
// This cloud function is designed to process clean-up in a batch for efficiency.
exports.processRankAdded =
    functions.database.ref('Leaderboard/Map/{MapType}/{MapID}/Top/Ranks/{RankID}')
    .onCreate(snapshot => {
  console.log("========= Rank Record Added =========");
  return updateLastCleanupTimestamp(snapshot).then(result => {
    // If the transaction is aborted because the last update timestamp is too closed to current
    // time, do nothing.  Otherwise, start the clean-up process.
    if (result.committed) {
      return processCleanup(snapshot.ref.parent);
    } else {
      return console.log("Skip clean-up process");
    }
  });
});

// This cloud function is to limit the number of children in under the following database url
//   Leaderboard/Map/{MapType}/{MapID}/Top/SharedReplays/
// Also, this function removes the replay data from the cloud storage if it is no longer referenced
// by the database.
// This cloud function is designed to process clean-up in a batch for efficiency.
exports.processSharedReplayAdded =
    functions.database.ref('Leaderboard/Map/{MapType}/{MapID}/Top/SharedReplays/{RankID}')
    .onCreate(snapshot => {
  console.log("========= Shared Replay Record Added =========");

  return updateLastCleanupTimestamp(snapshot).then(result => {
    // If the transaction is aborted because the last update timestamp is too closed to current
    // time, do nothing.  Otherwise, start the clean-up process.
    if (result.committed) {
      return processCleanup(snapshot.ref.parent);
    } else {
      return console.log("Skip clean-up process");
    }
  });
});

// Reduce the freqency of the heavy lifting clean-up process through lastCleanup timestamp and
// predefined CLEANUP_MIN_INTV_IN_MS property.
function updateLastCleanupTimestamp(snapshot) {
  const lastCleanupTimestampRef = snapshot.ref.parent.child(DB_KEY_LAST_CLEANUP);
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

// Limit the number of ranks records or shared replay records in the database and optionally
// remove unreferenced replay files.
// This implementation assumes that a replay file is unreferenced if
// a) The rank record referencing it is removed and is marked as not shared
// b) The shared replay record referencing it is removed
// The assumption is based on that both rank record and shared replay record shares the same
// TARGET_MAX_RECORDS and only the reference in these records matter.
function processCleanup(dbRef) {
  console.log("Create promises to clean up " + dbRef);

  const bucket = admin.storage().bucket();

  var cleanupPromises = dbRef.orderByChild(DB_KEY_SCORE).once('value').then(snapshot => {
    const childCount = snapshot.numChildren();
    if (childCount > RECORDS_CLEANUP_THRESHOLD) {
      const promises = [];
      const recordToRemove = {};

      let childIndex = 0;
      // DataSnapshot.forEach preserves the ascending order by score from orderByChild(DB_KEY_SCORE)
      snapshot.forEach(function(childSnapshot) {
        // Keep the first few records
        if (childIndex++ <= TARGET_MAX_RECORDS) {
          return;
        }

        // Set the child record to null to remove it in the database
        recordToRemove[childSnapshot.key] = null;

        // If dbRef is for SharedReplays, delete old replay files as well.
        if (dbRef.key == DB_KEY_SHARED_REPLAYS) {
          const replayPath = childSnapshot.child(DB_KEY_DATA).child(DB_KEY_REPLAY_PATH).val();
          if (replayPath) {
            console.log("Removing file " + replayPath);
            // Add the promise to remove the replay record from the storage
            promises.push(bucket.file(replayPath).delete());
          }
        }
      });

      // Add the promise to remove extra childrens under dbRef
      promises.push(dbRef.update(recordToRemove));

      return Promise.all(promises).then(undefined, error =>{
        return console.log("createCleanupTasks for " + dbRef + " failed. ", error);
      });
    } else {
      return console.log("Not enough child to clean-up for " + dbRef
          + " ( " + childCount + " <= " + RECORDS_CLEANUP_THRESHOLD + " )");
    }
  });

  return cleanupPromises.then(undefined, error => {
    console.log("processCleanup failed. ", error);
  });
}

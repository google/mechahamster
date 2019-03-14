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

// Database key for
//   Leaderboard/Map/{MapType}/{MapID}/Top/SharedReplay/lastCleanupReplay
const DB_KEY_LAST_CLEANUP_REPLAY = "lastCleanupReplay";

// Database keys to storage url of the replay data.
//   Leaderboard/Map/{MapType}/{MapID}/Top/SharedReplay/{RankID}/data/replayPath
const DB_KEY_REPLAY_PATH = "data/replayPath";

// Database key of the time of the rank record.
//   Leaderboard/Map/{MapType}/{MapID}/Top/SharedReplay/{RankID}/score
const DB_KEY_SCORE = "score";

// Minimum time between two cleanup process in milliseconds (1 minutes)
// The cloud function aborts if it is triggered less than this amount of time since last cleanup.
const CLEANUP_MIN_INTV_IN_MS = 1000 * 60;

// Target number of records to keep for each map.
const TARGET_MAX_RECORDS = 5;

// Start to clean up if the amount of record exceeds this number.
const RECORDS_CLEANUP_THRESHOLD = 10;

// This cloud function is to limit the number of replay data stored in the database and cloud
// storage.
// It is triggered when a new record is added to the database.
// It is designed to cleanup in a batch for efficiency.
exports.cleanupReplay =
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

// Reduce the frequency of the heavy lifting clean-up process using lastCleanupReplay timestamp.
function updateLastCleanupTimestamp(snapshot) {
  // Navigate to Leaderboard/Map/{MapType}/{MapID}/Top/lastCleanupReplay
  const lastCleanupTimestampRef = snapshot.ref.parent.parent.child(DB_KEY_LAST_CLEANUP_REPLAY);
  // Use transaction to update timestamp to prevent race condition.
  return lastCleanupTimestampRef.transaction(snapshot => {
    const timestamp = new Date().getTime();
    if (snapshot !== null && (timestamp - snapshot) <= CLEANUP_MIN_INTV_IN_MS) {
      // Abort the transaction if this is triggered too soon. This returns an "undefined" result.
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
      // DataSnapshot.forEach iterates in the ascending order from orderByChild(DB_KEY_SCORE)
      snapshot.forEach(function(childSnapshot) {
        // Keep the first few records
        if (childIndex++ <= TARGET_MAX_RECORDS) {
          return;
        }

        // Set the child record to null to remove it from the database
        recordToRemove[childSnapshot.key] = null;

        // Delete old replay files as well.
        const replayPath = childSnapshot.child(DB_KEY_REPLAY_PATH).val();
        if (replayPath) {
          console.log("Removing file " + replayPath);
          // Add the promise to remove the replay record from the storage
          promises.push(bucket.file(replayPath).delete());
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

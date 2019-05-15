// =================================================================================================
// Copyright 2018 DataArt, Inc.
// -------------------------------------------------------------------------------------------------
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this work except in compliance with the License.
// You may obtain a copy of the License in the LICENSE file, or at:
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// =================================================================================================
const chai = require('chai'),
    chaiHttp = require('chai-http'),
    config = require('config'),
    _ = require('lodash');
    Promise = require('bluebird');
chai.use(chaiHttp);

function events(filter, count) {
    return chai.request(config.get('seq.url'))
        .get(config.get('seq.events_route'))
        .query({ count: count || 50, filter });
}

function getEvents(filter, count, timeout) {

    let timed_out;
    setTimeout( () => { timed_out = true }, timeout);

    return new Promise( (resolve, reject) => {
        (function run() {
            Promise.delay(1000)
                .then( () => events(filter))
                .then( (res) => {
                    const logs = res.body;
                    if (logs.length < count) throw new Error(`expected to find at least ${count} logs in SEQ using '${filter}' filter but got only ${logs.length}`);
                    else resolve(logs);
                })
                .catch( (err) => {
                    if (timed_out) reject(err);
                    else run();
                });
        }());
    });
}

module.exports = {
    getEvents
};

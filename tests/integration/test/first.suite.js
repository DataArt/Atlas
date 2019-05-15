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
    expect = chai.expect,
    chaiHttp = require('chai-http'),
    config = require('config'),
    moment = require('moment'),
    uuidv1 = require('uuid/v1'),
    _ = require('lodash');
chai.use(chaiHttp);

const { getEvents } = require('./helpers/seq.helper');

describe('First Suite', () => {

    const services = ['Discovery', 'Alpha', 'Beta'];

    services.forEach((serviceName) => {

        it(`${serviceName}Service || ${config.get(`services.${serviceName}.version_route`)} || should return 200`, function(done) {

            chai.request(config.get(`services.${serviceName}.url`))
                .get(config.get(`services.${serviceName}.version_route`))
                .end((err, res) => {
                    expect(err).to.be.null;
                    expect(res).to.have.status(200);
                    done();
                });
        });
    });

    it(`AlphaService | ${config.get('services.Alpha.web_api_route')} | should return 204 \
    and all services (alpha, beta, and discovery) should log messages to SEQ`, (done) => {

        const serviceKey = 'beta';
        const CorrelationId = uuidv1();
        const dateTime = moment().subtract(1, 'minute').format();
        const seqFilter = `CorrelationId = '${CorrelationId}' and @Timestamp > DateTime('${dateTime}')`;

        Promise.delay(0)
            .then( () =>
                chai.request(config.get('services.Alpha.url'))
                    .get(config.get('services.Alpha.web_api_route'))
                    .query({serviceKey})
                    .set('CorrelationId', CorrelationId)
            )
            .then((res) => {
                expect(res).to.have.status(204);
            })
            .then( () =>
                getEvents(seqFilter, 3, 10000)
            )
            .then( (logs) =>
                _.forEach(['AlphaService', 'BetaService', 'DiscoveryService'], (serviceName) => {
                    expect(
                        _.filter(logs, (log) => _.some(log.Properties, {'Name': 'ServiceName', 'Value': serviceName})),
                        `'${seqFilter}' used SEQ filter. There is no log message for ${serviceName} service`
                    ).to.have.lengthOf(1);
                })
            )
            .then( () => done())
            .catch( (err) => done(err));
    });

    it(`AlphaService | ${config.get('services.Alpha.esb_route')} | should post message to ESB 
    and BetaService should consume it`, (done) => {

        let old_amount_of_consumed_messages;

        Promise.delay(0)
            .then( () =>
                chai.request(config.get('services.Beta.url'))
                    .get(config.get('services.Beta.state_route'))
            )
            .then((res) => {
                old_amount_of_consumed_messages = res.body;
            })
            .then( () =>
                chai.request(config.get('services.Alpha.url'))
                    .get(config.get('services.Alpha.esb_route'))
            )
            .then((res) => {
                expect(res).to.have.status(204);
            })
            .then( () => Promise.delay(1000))
            .then( () =>
                chai.request(config.get('services.Beta.url'))
                    .get(config.get('services.Beta.state_route'))
            )
            .then((res) => {
                const new_amount_of_consumed_messages = res.body;
                expect(new_amount_of_consumed_messages).to.equal(old_amount_of_consumed_messages + 1);
            })
            .then( () => done())
            .catch( (err) => done(err));
    });
});

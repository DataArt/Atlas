#region License
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
#endregion
using System.Threading;
using System.Threading.Tasks;
using DataArt.Atlas.Infrastructure.Startup;

#if NET452
using Serilog;
#endif

#if NETSTANDARD2_0
using DataArt.Atlas.Infrastructure.Logging;
using Microsoft.Extensions.Logging;
#endif

namespace DataArt.Atlas.Infrastructure.State
{
    public abstract class StateService<T> : Startable, IStateService<T>
    {
#if NETSTANDARD2_0
        private readonly ILogger logger = AtlasLogging.CreateLogger<StateService<T>>();
#endif

        private readonly object lockObject = new object();
        private StateModel<T> currentStateModel;

        public T State => currentStateModel.State;

        public void Update(StateModel<T> stateModel)
        {
            lock (lockObject)
            {
                if (currentStateModel == null || stateModel.UpdatedOn > currentStateModel.UpdatedOn)
                {
                    currentStateModel = stateModel;
                }
                else
                {
#if NET452
                    Log.Warning("Attempt to call update at state service {StateServiceName} with an obsolete value: {CurrentStateUpdatedOn} => {ObsoleteStateUpdatedOn}", GetType().Name, currentStateModel.UpdatedOn, stateModel.UpdatedOn);
#endif

#if NETSTANDARD2_0
                    logger.LogWarning("Attempt to call update at state service {StateServiceName} with an obsolete value: {CurrentStateUpdatedOn} => {ObsoleteStateUpdatedOn}", GetType().Name, currentStateModel.UpdatedOn, stateModel.UpdatedOn);
#endif
                }
            }
        }

        protected abstract Task<StateModel<T>> GetInitialStateAsync(CancellationToken cancellationToken);

        protected override async Task StartInternalAsync(CancellationToken cancellationToken)
        {
            Update(await GetInitialStateAsync(cancellationToken));
        }
    }
}

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

#if NET452
using System;
using System.Collections.Generic;
using Autofac;
using Serilog;

namespace DataArt.Atlas.Core.Shell
{
    internal static class AutofacExstensions
    {
        public static void GracefulDispose(this IContainer container)
        {
            using (container)
            {
                var disposables = container?.Resolve<IEnumerable<IDisposable>>();
                if (disposables != null)
                {
                    foreach (var disposable in disposables)
                    {
                        try
                        {
                            disposable?.Dispose();
                        }
                        catch (Exception e)
                        {
                            Log.Error(e, "Container component dispose exception: {@Disposable}", disposable);
                        }
                    }
                }
            }
        }
    }
}
#endif
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
namespace DataArt.Atlas.Infrastructure.Extensions
{
    public static class DataExtensions
    {
        public static string NormalizeBic(this string bicString)
        {
            // BIC must be 11 characters long. If last 3 characters are absent them must be replaced with 'X'
            if (!string.IsNullOrWhiteSpace(bicString))
            {
                var result = bicString.Trim();
                return result.Length == 8 ? result.PadRight(11, 'X') : result;
            }

            return null;
        }
    }
}

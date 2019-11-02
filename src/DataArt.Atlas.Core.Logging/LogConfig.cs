//--------------------------------------------------------------------------------------------------
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
//--------------------------------------------------------------------------------------------------
using System;
using DataArt.Atlas.Configuration.Settings;
using Serilog;
using Serilog.Core;
using LogEventLevel = Serilog.Events.LogEventLevel;

namespace DataArt.Atlas.Logging
{
    public static class LogConfig
    {
        private static readonly LoggingLevelSwitch LoggingLevelSwitch;

        static LogConfig()
        {
            LoggingLevelSwitch = new LoggingLevelSwitch(LogEventLevel.Verbose);
        }

        public static ILogger Configure(LoggingSettings settings, params ILogEventEnricher[] enrichers)
        {
            return ConfigureInternal(new LoggerConfiguration(), settings, enrichers);
        }

        public static ILogger Configure(this LoggerConfiguration loggerConfiguration, LoggingSettings settings, params ILogEventEnricher[] enrichers)
        {
            return ConfigureInternal(loggerConfiguration, settings, enrichers);
        }

        public static void SetLoggingLevel(LogEventLevel logLevel)
        {
            LoggingLevelSwitch.MinimumLevel = logLevel;
        }

        private static ILogger ConfigureInternal(LoggerConfiguration loggerConfiguration, LoggingSettings settings, params ILogEventEnricher[] enrichers)
        {
            LoggingLevelSwitch.MinimumLevel =
                settings?.Verbosity == null
                ? LogEventLevel.Verbose
                : (LogEventLevel)Enum.Parse(typeof(Configuration.Settings.LogEventLevel), settings.Verbosity.ToString());

            loggerConfiguration
                .MinimumLevel.ControlledBy(LoggingLevelSwitch)
                .Enrich.WithMachineName()
                .Enrich.With(new CorrelationIdEnricher());

            if (enrichers != null)
            {
                loggerConfiguration.Enrich.With(enrichers);
            }

            if (settings != null)
            {
                if (!string.IsNullOrWhiteSpace(settings.Environment))
                {
                    loggerConfiguration.Enrich.With(new PropertyEnricher("Environment", settings.Environment));
                }

                if (!string.IsNullOrWhiteSpace(settings.ApplicationName))
                {
                    loggerConfiguration.Enrich.With(new PropertyEnricher("ApplicationName", settings.ApplicationName));
                }

                if (!string.IsNullOrEmpty(settings.SinkUrl))
                {
                    loggerConfiguration.WriteTo.Seq(
                        settings.SinkUrl,
                        apiKey: settings.ApiKey,
                        compact: true,
                        controlLevelSwitch: LoggingLevelSwitch);
                }
            }

            loggerConfiguration.Enrich.FromLogContext();

            var logger = loggerConfiguration.CreateLogger();

            Log.Logger = logger;

            return logger;
        }
    }
}

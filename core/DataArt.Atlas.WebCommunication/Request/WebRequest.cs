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
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DataArt.Atlas.CallContext.Correlation;
using DataArt.Atlas.Infrastructure.Exceptions;
using DataArt.Atlas.ServiceDiscovery;
using DataArt.Atlas.WebCommunication.Exceptions;
using DataArt.Atlas.WebCommunication.HttpError;
using Flurl;
using Flurl.Http;
using Flurl.Http.Configuration;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
#if NET452
using Serilog;
#endif

#if NETSTANDARD2_0
using DataArt.Atlas.Infrastructure.Logging;
using Microsoft.Extensions.Logging;
#endif

namespace DataArt.Atlas.WebCommunication.Request
{
    internal sealed class WebRequest : IGetRequest, IPostRequest, IPutRequest, IDeleteRequest
    {
#if NETSTANDARD2_0
        private readonly ILogger logger = AtlasLogging.CreateLogger<WebRequest>();
#endif
        private readonly Dictionary<string, object> headers = new Dictionary<string, object>();
        private readonly AsyncRetryPolicy retryPolicy;
        private readonly IFlurlClientFactory flurlClientFactory;
        private readonly DefaultFlurlHttpSettings defaultFlurlHttpSettings;
        private TimeSpan timeout = TimeSpan.FromSeconds(100);
        private Url url;
        private object body;

        public WebRequest(
            string resource,
            string serviceKey,
            IServiceDiscovery serviceDiscovery,
            IFlurlClientFactory flurlClientFactory,
            DefaultFlurlHttpSettings defaultFlurlHttpSettings)
        {
            if (serviceKey == null)
            {
                throw new ArgumentNullException(nameof(serviceKey));
            }

            if (resource == null)
            {
                throw new ArgumentNullException(nameof(resource));
            }

            this.flurlClientFactory = flurlClientFactory;
            this.defaultFlurlHttpSettings = defaultFlurlHttpSettings;

            url = serviceDiscovery.ResolveServiceUrl(serviceKey).AbsoluteUri.AppendPathSegment(resource);

            retryPolicy = Policy.Handle<CommunicationException>()
                .RetryAsync((exception, retryCount) =>
                {
#if NET452
                    Log.Warning(exception, "Communication exception occured to {ServiceKey} - {ServiceUrl}", serviceKey, url.Path);
#endif

#if NETSTANDARD2_0
                    logger.LogWarning(exception, "Communication exception occured to {ServiceKey} - {ServiceUrl}", serviceKey, url.Path);
#endif
                    var serviceUri = serviceDiscovery.ResolveServiceUrl(serviceKey);
                    url = new UriBuilder(Url.Decode(url, false)) { Host = serviceUri.Host, Port = serviceUri.Port }.Uri;

#if NET452
                    Log.Warning("A new url to {ServiceKey} was resolved on retry - {ServiceUrl}", serviceKey, url.Path);
#endif

#if NETSTANDARD2_0
                    logger.LogWarning("A new url to {ServiceKey} was resolved on retry - {ServiceUrl}", serviceKey, url.Path);
#endif
                });
        }

        public void SetHeader(string name, object value)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            headers[name] = value ?? throw new ArgumentNullException(nameof(value));
        }

        public void SetTimeout(TimeSpan value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            timeout = value;
        }

#region IGetRequest Implementation

        IGetRequest IRequest<IGetRequest>.AddUrlSegment(string value)
        {
            return (IGetRequest)AddUrlSegment(value);
        }

        IGetRequest IRequest<IGetRequest>.AddUriParameter(string name, object value)
        {
            return (IGetRequest)AddUriParameter(name, value);
        }

        IGetRequest IRequest<IGetRequest>.AddUriParameters(IDictionary<string, string> parameters)
        {
            return (IGetRequest)AddUriParameters(parameters);
        }

        IGetRequest IRequest<IGetRequest>.AddUriParameters(object valueToSetAsParams)
        {
            return (IGetRequest)AddUriParameters(valueToSetAsParams);
        }

        public Task GetAsync(CancellationToken cancellationToken)
        {
            return ExecuteRequestAsync(() => CreateRequest().GetAsync(cancellationToken));
        }

        public Task<TResponse> GetAsync<TResponse>(CancellationToken cancellationToken)
        {
            return ExecuteRequestAsync(() => CreateRequest().GetJsonAsync<TResponse>(cancellationToken));
        }

#endregion

#region IPostRequest Implementation

        IPostRequest IRequest<IPostRequest>.AddUrlSegment(string value)
        {
            return (IPostRequest)AddUrlSegment(value);
        }

        IPostRequest IRequest<IPostRequest>.AddUriParameter(string name, object value)
        {
            return (IPostRequest)AddUriParameter(name, value);
        }

        IPostRequest IRequest<IPostRequest>.AddUriParameters(IDictionary<string, string> parameters)
        {
            return (IPostRequest)AddUriParameters(parameters);
        }

        IPostRequest IRequest<IPostRequest>.AddUriParameters(object valueToSetAsParams)
        {
            return (IPostRequest)AddUriParameters(valueToSetAsParams);
        }

        public Task PostAsync(CancellationToken cancellationToken)
        {
            return ExecuteRequestAsync(() => CreateRequest().PostJsonAsync(body, cancellationToken));
        }

        IPostRequest IPostRequest.AddBody(object value)
        {
            return (IPostRequest)AddBody(value);
        }

        public Task<TResponse> PostAsync<TResponse>(CancellationToken cancellationToken)
        {
            return ExecuteRequestAsync(() => CreateRequest().PostJsonAsync(body, cancellationToken).ReceiveJson<TResponse>());
        }

#endregion

#region IPutRequest Implementation

        IPutRequest IRequest<IPutRequest>.AddUrlSegment(string value)
        {
            return (IPutRequest)AddUrlSegment(value);
        }

        IPutRequest IRequest<IPutRequest>.AddUriParameter(string name, object value)
        {
            return (IPutRequest)AddUriParameter(name, value);
        }

        IPutRequest IRequest<IPutRequest>.AddUriParameters(IDictionary<string, string> parameters)
        {
            return (IPutRequest)AddUriParameters(parameters);
        }

        IPutRequest IRequest<IPutRequest>.AddUriParameters(object valueToSetAsParams)
        {
            return (IPutRequest)AddUriParameters(valueToSetAsParams);
        }

        public Task PutAsync(CancellationToken cancellationToken)
        {
            return ExecuteRequestAsync(() => CreateRequest().PutJsonAsync(body, cancellationToken));
        }

        IPutRequest IPutRequest.AddBody(object value)
        {
            return (IPutRequest)AddBody(value);
        }

        public Task<TResponse> PutAsync<TResponse>(CancellationToken cancellationToken)
        {
            return ExecuteRequestAsync(() => CreateRequest().PutJsonAsync(body, cancellationToken).ReceiveJson<TResponse>());
        }

#endregion

#region IDeleteRequest Implementation

        IDeleteRequest IRequest<IDeleteRequest>.AddUrlSegment(string value)
        {
            return (IDeleteRequest)AddUrlSegment(value);
        }

        IDeleteRequest IRequest<IDeleteRequest>.AddUriParameter(string name, object value)
        {
            return (IDeleteRequest)AddUriParameter(name, value);
        }

        IDeleteRequest IRequest<IDeleteRequest>.AddUriParameters(IDictionary<string, string> parameters)
        {
            return (IDeleteRequest)AddUriParameters(parameters);
        }

        IDeleteRequest IRequest<IDeleteRequest>.AddUriParameters(object valueToSetAsParams)
        {
            return (IDeleteRequest)AddUriParameters(valueToSetAsParams);
        }

        public Task DeleteAsync(CancellationToken cancellationToken)
        {
            return ExecuteRequestAsync(() => CreateRequest().DeleteAsync(cancellationToken));
        }

        public Task<TResponse> DeleteAsync<TResponse>(CancellationToken cancellationToken)
        {
            return ExecuteRequestAsync(() => CreateRequest().DeleteAsync(cancellationToken).ReceiveJson<TResponse>());
        }

#endregion

        private static async Task<Exception> MapErrorResponseToExceptionAsync(HttpResponseMessage response)
        {
            var error = await ReadAsHttpErrorAsync(response);

            switch (response.StatusCode)
            {
                case HttpStatusCode.BadRequest:
                    if (error.Type == HttpErrorType.Validation)
                    {
                        return new ApiValidationException(error.Message);
                    }

                    return new BadRequestException(error.Message);
                case HttpStatusCode.Conflict:
                    return new ConflictException(error.Message);
                case HttpStatusCode.NotFound:
                    return new NotFoundException(error.Message);
                case HttpStatusCode.Forbidden:
                    return new AuthorizationException(error.Message);
                case HttpStatusCode.Unauthorized:
                    return new AuthenticationException(error.Message);
                case HttpStatusCode.InternalServerError:
                    return new InternalServerException(error.Message);
                default:
                    return new CommunicationException(error.Message, response);
            }
        }

        private static async Task<HttpError.HttpError> ReadAsHttpErrorAsync(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();

            try
            {
                var error = JsonConvert.DeserializeObject<HttpError.HttpError>(content);

                if (error != null)
                {
                    return error;
                }

                return new HttpError.HttpError { Message = content };
            }
            catch (JsonReaderException)
            {
                return new HttpError.HttpError { Message = content };
            }
        }

        private IRequest AddUrlSegment(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            url.AppendPathSegment(value);
            return this;
        }

        private IRequest AddUriParameter(string name, object value)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            url.SetQueryParam(name, value);
            return this;
        }

        private IRequest AddUriParameters(IDictionary<string, string> parameters)
        {
            foreach (var parameter in parameters)
            {
                AddUriParameter(parameter.Key, parameter.Value);
            }

            return this;
        }

        private IRequest AddUriParameters(object valueToSetAsParams)
        {
            if (valueToSetAsParams == null)
            {
                throw new ArgumentNullException(nameof(valueToSetAsParams));
            }

            url.SetQueryParams(valueToSetAsParams);

            return this;
        }

        private IRequest AddBody(object obj)
        {
            body = obj;
            return this;
        }

        private IFlurlRequest CreateRequest()
        {
            var flurlRequest = new FlurlRequest(url) { Client = flurlClientFactory.Get(url) };

            flurlRequest.WithHeader(CorrelationContext.CorrelationIdName, CorrelationContext.CorrelationId);

            if (headers != null)
            {
                foreach (var header in headers)
                {
                    flurlRequest.WithHeader(header.Key, header.Value);
                }
            }

            flurlRequest.Settings = defaultFlurlHttpSettings.Create();

            flurlRequest.WithTimeout(timeout);

            return flurlRequest;
        }

        private Task<TResult> ExecuteRequestAsync<TResult>(Func<Task<TResult>> request)
        {
            return retryPolicy.ExecuteAsync(async () =>
            {
                try
                {
                    return await request();
                }
                catch (FlurlHttpException exception)
                {
                    if (exception is FlurlHttpTimeoutException)
                    {
                        throw new CommunicationException(exception.Message);
                    }

                    if (exception is FlurlParsingException)
                    {
                        throw new InternalServerException(exception.Message);
                    }

                    var call = exception.Call;

                    if (call?.Response != null)
                    {
                        throw await MapErrorResponseToExceptionAsync(call.Response);
                    }

                    if (call?.Exception != null)
                    {
                        if (call.Exception is OperationCanceledException)
                        {
                            throw call.Exception;
                        }

                        // case in point: server is not responding (e.g. isn't up yet)
                        throw new CommunicationException(call.Exception.Message);
                    }

                    throw new CommunicationException("Unknown error");
                }
            });
        }
    }
}

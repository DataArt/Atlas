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
using System.Net;
using System.Net.Http;
using System.Web.Http.Filters;
using DataArt.Atlas.Infrastructure.Exceptions;
using DataArt.Atlas.WebCommunication.HttpError;

namespace DataArt.Atlas.Core.Application.Http
{
    internal sealed class ApiExceptionHandlingFilter : ActionFilterAttribute
    {
        public const string HandledExceptionKey = "HandledException";

        private readonly bool showErrorDetails;
        private readonly bool isGateway;

        public ApiExceptionHandlingFilter(bool showErrorDetails, bool isGateway)
        {
            this.showErrorDetails = showErrorDetails;
            this.isGateway = isGateway;
        }

        public override void OnActionExecuted(HttpActionExecutedContext context)
        {
            base.OnActionExecuted(context);

            if (context.Exception != null)
            {
                var exception = GetMostInnerException(context.Exception);
                context.Response = GetResponse(context.Request, exception);
                context.Request.Properties[HandledExceptionKey] = exception;
            }
        }

        private static HttpStatusCode GetStatusCode(Exception exception)
        {
            if (exception is BadRequestException || exception is ArgumentException || exception is NotSupportedException)
            {
                return HttpStatusCode.BadRequest;
            }

            if (exception is ConflictException)
            {
                return HttpStatusCode.Conflict;
            }

            if (exception is NotFoundException)
            {
                return HttpStatusCode.NotFound;
            }

            if (exception is AuthenticationException)
            {
                return HttpStatusCode.Unauthorized;
            }

            if (exception is AuthorizationException)
            {
                return HttpStatusCode.Forbidden;
            }

            return HttpStatusCode.InternalServerError;
        }

        private static HttpErrorType GetErrorType(Exception exception)
        {
            if (exception is ApiValidationException)
            {
                return HttpErrorType.Validation;
            }

            return HttpErrorType.Other;
        }

        private static Exception GetMostInnerException(Exception exception)
        {
            // we should get the most inner exception (thank you Tasks)
            // obviously Exception.GetBaseException() not in all cases makes the same
            while (exception.InnerException != null)
            {
                exception = exception.InnerException;
            }

            return exception;
        }

        private HttpResponseMessage GetResponse(HttpRequestMessage request, Exception exception)
        {
            var statusCode = GetStatusCode(exception);
            var errorType = GetErrorType(exception);

            if (isGateway)
            {
                var showMessage = showErrorDetails || errorType == HttpErrorType.Validation;
                return request.CreateResponse(statusCode, showMessage ? exception.Message : string.Empty);
            }

            var error = new HttpError { Message = exception.Message, Type = errorType };
            return request.CreateResponse(statusCode, error);
        }
    }
}
#endif
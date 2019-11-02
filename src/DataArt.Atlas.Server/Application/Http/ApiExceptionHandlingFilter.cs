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
using DataArt.Atlas.Client.HttpError;
using DataArt.Atlas.Core.Exceptions;
#if NETSTANDARD2_0
using System;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DataArt.Atlas.Core.Application.Http
{
    internal sealed class ApiExceptionHandlingFilter : IExceptionFilter
    {
        public const string HandledExceptionKey = "HandledException";

        private readonly bool showErrorDetails;
        private readonly bool isGateway;

        public ApiExceptionHandlingFilter(bool showErrorDetails, bool isGateway)
        {
            this.showErrorDetails = showErrorDetails;
            this.isGateway = isGateway;
        }

        public void OnException(ExceptionContext context)
        {
            var exception = GetMostInnerException(context.Exception);
            context.HttpContext.Items[HandledExceptionKey] = exception;
            context.Result = GetResponse(exception);
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

        private IActionResult GetResponse(Exception exception)
        {
            var statusCode = (int)GetStatusCode(exception);
            var errorType = GetErrorType(exception);

            if (isGateway)
            {
                var showMessage = showErrorDetails || errorType == HttpErrorType.Validation;
                return new ObjectResult(showMessage ? exception.Message : string.Empty)
                {
                    StatusCode = statusCode
                };
            }

            var error = new HttpError { Message = exception.Message, Type = errorType };
            return new ObjectResult(error)
            {
                StatusCode = statusCode
            };
        }
    }
}
#endif
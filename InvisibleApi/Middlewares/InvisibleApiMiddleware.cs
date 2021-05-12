// ---------------------------------------------------------------
// Copyright (c) Hassan Habib All rights reserved.
// Licensed under the MIT License.
// See License.txt in the project root for license information.
// ---------------------------------------------------------------

using InvisibleApi.Attributes;
using InvisibleApi.Models.InvisibleApiConfigurations;
using InvisibleApi.Models.InvisibleApiProfiles;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InvisibleApi.Middlewares
{
    public class InvisibleApiMiddleware
    {
        private readonly RequestDelegate next;
        private readonly List<InvisibleApiConfiguration> invisibleApiConfigurations;
        private readonly List<InvisibleApiProfile> invisibleApiProfiles;

        public InvisibleApiMiddleware(
            List<InvisibleApiConfiguration> invisibleApiConfigurations,
            List<InvisibleApiProfile> invisibleApiProfiles,
            RequestDelegate next)
        {
            this.invisibleApiConfigurations = invisibleApiConfigurations;
            this.invisibleApiProfiles = invisibleApiProfiles;
            this.next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var endpoint = context.Features?.Get<IEndpointFeature>()?.Endpoint;
            var attribute = endpoint?.Metadata.GetMetadata<InvisibleApiAttribute>();

            if (FailedConfigMatch(context.Request))
                context.Response.StatusCode = StatusCodes.Status404NotFound;
            else if (FailedProfileMatch(attribute, context.Request))
                context.Response.StatusCode = StatusCodes.Status404NotFound;
            else
                await next(context);
        }

        private bool FailedConfigMatch(HttpRequest request)
        {
            if (invisibleApiConfigurations is null)
                return false;

            var hasMatchingConfigPath = false;

            for (var cInd = 0; cInd < invisibleApiConfigurations.Count; cInd++)
            {
                var config = invisibleApiConfigurations[cInd];

                if (config.Endpoint != request.Path || config.HttpVerb != request.Method)
                    continue;

                hasMatchingConfigPath = true;

                if (request.Headers[config.Header] == config.Value)
                    return false;
            }

            return hasMatchingConfigPath;
        }

        private bool FailedProfileMatch(InvisibleApiAttribute attribute, HttpRequest request)
        {
            if (attribute is null || invisibleApiProfiles is null)
                return false;

            var hasMatchingProfile = false;

            for (var aInd = 0; aInd < attribute.Profiles.Length; aInd++)
            {
                for (var pInd = 0; pInd < invisibleApiProfiles.Count; pInd++)
                {
                    if (invisibleApiProfiles[pInd].Name != attribute.Profiles[aInd])
                        continue;

                    hasMatchingProfile = true;

                    var matchingProfile = invisibleApiProfiles[pInd];

                    if (request.Headers[matchingProfile.Header] == matchingProfile.Value)
                        return false;
                }
            }

            return hasMatchingProfile;
        }
    }
}

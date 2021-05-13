// ---------------------------------------------------------------
// Copyright (c) Hassan Habib All rights reserved.
// Licensed under the MIT License.
// See License.txt in the project root for license information.
// ---------------------------------------------------------------

using InvisibleApi.Middlewares;
using InvisibleApi.Models.InvisibleApiConfigurations;
using InvisibleApi.Models.InvisibleApiProfiles;
using Microsoft.AspNetCore.Builder;
using System.Collections.Generic;

namespace InvisibleApi.Extensions
{
    public static class InvisibleApiApplicationBuilderExtension
    {
        public static IApplicationBuilder UseInvisibleApis(
            this IApplicationBuilder app,
            List<InvisibleApiConfiguration> invisibleApiDetails,
            List<InvisibleApiProfile> invisibleApiProfiles)
        {
            object[] parameters = { invisibleApiDetails, invisibleApiProfiles };

            return app.UseMiddleware<InvisibleApiMiddleware>(parameters);
        }
    }
}

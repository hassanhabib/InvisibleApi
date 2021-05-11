// ---------------------------------------------------------------
// Copyright (c) Hassan Habib All rights reserved.
// Licensed under the MIT License.
// See License.txt in the project root for license information.
// ---------------------------------------------------------------

using System.Collections.Generic;
using InvisibleApi.Middlewares;
using InvisibleApi.Models.InvisibleApiConfigurations;
using Microsoft.AspNetCore.Builder;

namespace InvisibleApi.Extensions
{
    public static class InvisibleApiApplicationBuilderExtension
    {
        public static IApplicationBuilder UseInvisibleApis(
            this IApplicationBuilder app, 
            List<InvisibleApiConfiguration> invisibleApiDetails) =>
                app.UseMiddleware<InvisibleApiMiddleware>(invisibleApiDetails);
    }
}

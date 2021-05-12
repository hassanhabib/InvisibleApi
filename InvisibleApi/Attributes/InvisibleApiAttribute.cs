// ---------------------------------------------------------------
// Copyright (c) Hassan Habib All rights reserved.
// Licensed under the MIT License.
// See License.txt in the project root for license information.
// ---------------------------------------------------------------

using System;

namespace InvisibleApi.Attributes
{
    public class InvisibleApiAttribute : Attribute
    {
        public string[] Profiles { get; set; }

        public InvisibleApiAttribute(params string[] profiles) =>
            Profiles = profiles;
    }
}

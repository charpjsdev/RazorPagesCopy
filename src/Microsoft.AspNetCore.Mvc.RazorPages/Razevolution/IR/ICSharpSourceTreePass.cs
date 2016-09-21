﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.Mvc.RazorPages.Razevolution.IR
{
    public interface ICSharpSourceTreePass : IRazorEngineFeature
    {
        int Order { get; }

        CSharpSourceTree Execute(RazorCodeDocument document, CSharpSourceTree sourceTree);
    }
}

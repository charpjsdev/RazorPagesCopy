﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.FileProviders;

namespace Microsoft.AspNetCore.Mvc.RazorPages.Infrastructure
{
    public class RazorPageActionDescriptorProvider : IActionDescriptorProvider
    {
        private readonly IFileProvider _fileProvider;

        public RazorPageActionDescriptorProvider(IRazorPagesFileProviderAccessor fileProvider)
        {
            _fileProvider = fileProvider.FileProvider;
        }

        public int Order { get; set; }

        public void OnProvidersExecuting(ActionDescriptorProviderContext context)
        {
            foreach (var file in EnumerateFiles())
            {
                if (string.Equals(Path.GetExtension(file.ViewEnginePath), ".razor", StringComparison.Ordinal))
                {
                    context.Results.Add(CreateActionDescriptor(file));
                }
            }
        }

        public void OnProvidersExecuted(ActionDescriptorProviderContext context)
        {
        }

        private RazorPageActionDescriptor CreateActionDescriptor(RazorPageFileInfo file)
        {
            var actionDescriptor = new RazorPageActionDescriptor()
            {
                AttributeRouteInfo = new AttributeRouteInfo()
                {
                    Template = file.ViewEnginePath.Substring(1, file.ViewEnginePath.Length - (Path.GetExtension(file.ViewEnginePath).Length + 1)),
                },
                DisplayName = $"Page: {file.ViewEnginePath}",
                RelativePath = "Pages" + file.ViewEnginePath,
                ViewEnginePath = file.ViewEnginePath,
            };

            return actionDescriptor;
        }

        private IEnumerable<RazorPageFileInfo> EnumerateFiles()
        {
            var directory = _fileProvider.GetDirectoryContents("Pages");
            return EnumerateFiles(directory, "/");
        }

        private IEnumerable<RazorPageFileInfo> EnumerateFiles(IDirectoryContents directory, string prefix)
        {
            if (directory.Exists)
            {
                foreach (var file in directory)
                {
                    if (file.IsDirectory)
                    {
                        var children = EnumerateFiles(_fileProvider.GetDirectoryContents(file.PhysicalPath), prefix + file.Name + "/");
                        foreach (var child in children)
                        {
                            yield return child;
                        }
                    }
                    else
                    {
                        yield return new RazorPageFileInfo(file, prefix + file.Name);
                    }
                }
            }
        }

        private class RazorPageFileInfo
        {
            public RazorPageFileInfo(IFileInfo fileInfo, string viewEnginePath)
            {
                FileInfo = fileInfo;
                ViewEnginePath = viewEnginePath;
            }

            public IFileInfo FileInfo { get; }

            public string ViewEnginePath { get; }
        }
    }
}

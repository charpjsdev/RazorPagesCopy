﻿// Copyright(c) .NET Foundation.All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.RazorPages.Razevolution.IR;
using Microsoft.AspNetCore.Razor;
using Microsoft.AspNetCore.Razor.CodeGenerators;

namespace Microsoft.AspNetCore.Mvc.RazorPages.Razevolution.CSharpRendering
{
    public class DefaultCSharpDocumentGenerationPhase : ICodeDocumentGenerationPhase
    {
        private readonly RazorEngineHost _host;

        public DefaultCSharpDocumentGenerationPhase(RazorEngineHost host)
        {
            _host = host;
        }

        public RazorEngine Engine { get; set; }

        public void Execute(RazorCodeDocument document)
        {
            var sourceTree = document.GetSourceTree();

            if (sourceTree == null)
            {
                throw new InvalidOperationException("Need to create the source tree");
            }

            var csharpDocument = document.GetGeneratedCSharpDocument();

            if (csharpDocument == null)
            {
                var csharpRenderers = Engine.Features.OfType<ICSharpRenderer>();
                if (!csharpRenderers.Any())
                {
                    throw new InvalidOperationException("Need to create the csharp document");
                }

                var rendererOrchestrator = new CSharpRendererOrchestrator(csharpRenderers, _host, document.ErrorSink);
                csharpDocument = rendererOrchestrator.Render(sourceTree);

                document.SetGeneratedCSharpDocument(csharpDocument);
            }

            // TODO: Add csharp document passes?
        }

        // TODO: Should this be a feature?
        private class CSharpRendererOrchestrator
        {
            private readonly CSharpRenderingContext _generationContext;
            private readonly IList<ICSharpRenderer> _renderers;

            public CSharpRendererOrchestrator(
                IEnumerable<ICSharpRenderer> renderers,
                RazorEngineHost _host,
                ErrorSink errorSink)
            {
                _renderers = renderers.OrderBy(renderer => renderer.Order).ToList();
                _generationContext = new CSharpRenderingContext
                {
                    CodeLiterals = _host.GeneratedClassContext,
                    Writer = new CSharpCodeWriter(),
                    ErrorSink = errorSink,
                    Render = Render,
                };
            }

            public GeneratedCSharpDocument Render(CSharpSourceTree sourceTree)
            {
                Render(sourceTree.Children);

                var generatedCSharpDocument = new GeneratedCSharpDocument
                {
                    GeneratedCode = _generationContext.Writer.GenerateCode()
                };

                return generatedCSharpDocument;
            }

            private void Render(IList<ICSharpSource> sources)
            {
                for (var i = 0; i < sources.Count; i++)
                {
                    Render(sources[i]);
                }
            }

            private void Render(ICSharpSource source)
            {
                for (var i = 0; i < _renderers.Count; i++)
                {
                    if (_renderers[i].TryRender(source, _generationContext))
                    {
                        // Successfully rendered the source
                        return;
                    }
                }

                var sourceBlock = source as CSharpBlock;
                if (sourceBlock != null)
                {
                    Render(sourceBlock.Children);
                }
            }
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Reporting.Html.ViewModels;
using SJP.Schematic.Reporting.Html.ViewModels.Mappers;

namespace SJP.Schematic.Reporting.Html.Renderers
{
    internal sealed class SequencesRenderer : ITemplateRenderer
    {
        public SequencesRenderer(
            IDbConnection connection,
            IRelationalDatabase database,
            IHtmlFormatter formatter,
            IReadOnlyCollection<IDatabaseSequence> sequences,
            DirectoryInfo exportDirectory)
        {
            if (sequences == null || sequences.AnyNull())
                throw new ArgumentNullException(nameof(sequences));

            Sequences = sequences;

            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            Database = database ?? throw new ArgumentNullException(nameof(database));
            Formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
            ExportDirectory = exportDirectory ?? throw new ArgumentNullException(nameof(exportDirectory));
        }

        private IDbConnection Connection { get; }

        private IRelationalDatabase Database { get; }

        private IHtmlFormatter Formatter { get; }

        private IReadOnlyCollection<IDatabaseSequence> Sequences { get; }

        private DirectoryInfo ExportDirectory { get; }

        public async Task RenderAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var mapper = new MainModelMapper(Connection, Database);

            var sequenceViewModels = Sequences.Select(mapper.Map).ToList();
            var sequencesVm = new Sequences(sequenceViewModels);

            var renderedMain = Formatter.RenderTemplate(sequencesVm);

            var mainContainer = new Container(renderedMain, Database.IdentifierDefaults.Database, string.Empty);
            var renderedPage = Formatter.RenderTemplate(mainContainer);

            if (!ExportDirectory.Exists)
                ExportDirectory.Create();
            var outputPath = Path.Combine(ExportDirectory.FullName, "sequences.html");

            using (var writer = File.CreateText(outputPath))
                await writer.WriteAsync(renderedPage).ConfigureAwait(false);
        }
    }
}
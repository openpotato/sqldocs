#region SqlDocs - Copyright (C) 2023 STÜBER SYSTEMS GmbH
/*    
 *    SqlDocs
 *    
 *    Copyright (C) 2023 STÜBER SYSTEMS GmbH
 *
 *    This program is free software: you can redistribute it and/or modify
 *    it under the terms of the GNU Affero General Public License, version 3,
 *    as published by the Free Software Foundation.
 *
 *    This program is distributed in the hope that it will be useful,
 *    but WITHOUT ANY WARRANTY; without even the implied warranty of
 *    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *    GNU Affero General Public License for more details.
 *
 *    You should have received a copy of the GNU Affero General Public License
 *    along with this program. If not, see <http://www.gnu.org/licenses/>.
 *
 */
#endregion

using Enbrea.Progress;
using NGettext;
using SqlDocs.DatabaseProvider;
using SqlDocs.DataModel;
using SqlDocs.DocsGenerator;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SqlDocs
{
    public static class CommandHandlers
    {
        public static async Task BuildJson(DatabaseEngine dbEngine, string dbConnection, FileInfo dbSchemaFile)
        {
            await Execute(async (cancellationToken, cancellationEvent) =>
            {
                var progressReport = ProgressReportFactory.CreateProgressReport(ProgressUnit.Count);
                try
                {
                    progressReport.Start("Generate database schema");

                    var provider = DatabaseProviderFactory.CreateDatabaseProvider(dbEngine, dbConnection);
                    var dbSchema = await DbSchemaFactory.CreateDbSchemaAsync(provider, cancellationToken);

                    progressReport.Finish();

                    if (dbSchemaFile.Exists) 
                    {
                        progressReport.Start("Merge and update database schema file");

                        dbSchema = await DbSchemaFactory.LoadAndMergeDbSchemaAsync(dbSchemaFile, dbSchema, cancellationToken);
                        await dbSchema.SaveAsync(dbSchemaFile, cancellationToken);

                        progressReport.Finish();
                    }
                    else
                    {
                        progressReport.Start("Save database schema file");

                        await dbSchema.SaveAsync(dbSchemaFile, cancellationToken);

                        progressReport.Finish();
                    }

                    progressReport.Success($"Schema file {dbSchemaFile.Name} successfully generated or updated");
                }
                catch (Exception ex)
                {
                    progressReport.NewLine();
                    progressReport.Error($"Build failed. {ex.Message}");
                    Environment.ExitCode = 1;
                }
            });
        }

        public static async Task BuildJsonAndMkDocs(DatabaseEngine dbEngine, string dbConnection, FileInfo dbSchemaFile, DirectoryInfo outputFolder, string language)
        {
            await Execute(async (cancellationToken, cancellationEvent) =>
            {
                var progressReport = ProgressReportFactory.CreateProgressReport(ProgressUnit.Count);
                var catalog = new Catalog("SqlDocs", "./L11n", new CultureInfo(language));
                try
                {
                    progressReport.Start("Generate database schema");

                    var provider = DatabaseProviderFactory.CreateDatabaseProvider(dbEngine, dbConnection);
                    var dbSchema = await DbSchemaFactory.CreateDbSchemaAsync(provider, cancellationToken);

                    progressReport.Finish();

                    if (dbSchemaFile.Exists)
                    {
                        progressReport.Start("Merge and update database schema file");

                        dbSchema = await DbSchemaFactory.LoadAndMergeDbSchemaAsync(dbSchemaFile, dbSchema, cancellationToken);
                        await dbSchema.SaveAsync(dbSchemaFile, cancellationToken);

                        progressReport.Finish();
                    }
                    else
                    {
                        progressReport.Start("Save database schema file");

                        await dbSchema.SaveAsync(dbSchemaFile, cancellationToken);

                        progressReport.Finish();
                    }

                    progressReport.Success($"Schema file {dbSchemaFile.Name} successfully generated or updated");

                    progressReport.Start("Generate MkDocs project");

                    var docsGenerator = DocsGeneratorFactory.CreateMkDocsGenerator(dbEngine, catalog);

                    await docsGenerator.GenerateAsync(dbSchema, outputFolder);

                    progressReport.Finish();
                    progressReport.Success($"MkDocs project successfully generated or updated");

                }
                catch (Exception ex)
                {
                    progressReport.NewLine();
                    progressReport.Error($"Build failed. {ex.Message}");
                    Environment.ExitCode = 1;
                }
            });
        }

        public static async Task BuildMkDocs(DatabaseEngine dbEngine, FileInfo dbSchemaFile, DirectoryInfo outputFolder, string language)
        {
            await Execute(async (cancellationToken, cancellationEvent) =>
            {
                var progressReport = ProgressReportFactory.CreateProgressReport(ProgressUnit.Count);
                var catalog = new Catalog("SqlDocs", "./L11n", new CultureInfo(language));
                try
                {
                    progressReport.Start("Load database schema file");

                    var dbSchema = await DbSchemaFactory.LoadDbSchemaAsync(dbSchemaFile, cancellationToken);
                    await dbSchema.SaveAsync(dbSchemaFile, cancellationToken);

                    progressReport.Finish();

                    progressReport.Success($"Schema file {dbSchemaFile.Name} successfully loaded");

                    progressReport.Start("Generate MkDocs project");

                    var docsGenerator = DocsGeneratorFactory.CreateMkDocsGenerator(dbEngine, catalog);

                    await docsGenerator.GenerateAsync(dbSchema, outputFolder);

                    progressReport.Finish();
                    progressReport.Success($"MkDocs project successfully generated or updated");

                }
                catch (Exception ex)
                {
                    progressReport.NewLine();
                    progressReport.Error($"Build failed. {ex.Message}");
                    Environment.ExitCode = 1;
                }
            });
        }

        private static async Task Execute(Func<CancellationToken, EventWaitHandle, Task> action)
        {
            using var cancellationTokenSource = new CancellationTokenSource();
            using var cancellationEvent = new EventWaitHandle(false, EventResetMode.AutoReset);

            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                eventArgs.Cancel = true;
                cancellationTokenSource.Cancel();
                cancellationEvent.Set();
            };

            var stopwatch = new Stopwatch();

            stopwatch.Start();

            try
            {
                await action(cancellationTokenSource.Token, cancellationEvent);
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine($"Console failed. {ex.Message}");
                Environment.ExitCode = 1;
            }

            stopwatch.Stop();

            Console.WriteLine();
            Console.WriteLine($"Time elapsed: {stopwatch.Elapsed}.");
        }
    }
}
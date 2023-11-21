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

using Enbrea.Konsoli;
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
                var consoleWriter = ConsoleWriterFactory.CreateConsoleWriter(ProgressUnit.Count);
                try
                {
                    consoleWriter.StartProgress("Generate database schema");

                    var provider = DatabaseProviderFactory.CreateDatabaseProvider(dbEngine, dbConnection);
                    var dbSchema = await DbSchemaFactory.CreateDbSchemaAsync(provider, cancellationToken);

                    consoleWriter.FinishProgress();

                    if (dbSchemaFile.Exists)
                    {
                        consoleWriter.StartProgress("Merge and update database schema file");

                        dbSchema = await DbSchemaFactory.LoadAndMergeDbSchemaAsync(dbSchemaFile, dbSchema, cancellationToken);
                        await dbSchema.SaveAsync(dbSchemaFile, cancellationToken);

                        consoleWriter.FinishProgress();
                    }
                    else
                    {
                        consoleWriter.StartProgress("Save database schema file");

                        await dbSchema.SaveAsync(dbSchemaFile, cancellationToken);

                        consoleWriter.FinishProgress();
                    }

                    consoleWriter.Success($"Schema file {dbSchemaFile.Name} successfully generated or updated");
                }
                catch (Exception ex)
                {
                    consoleWriter.NewLine();
                    consoleWriter.Error($"Build failed. {ex.Message}");
                    Environment.ExitCode = 1;
                }
            });
        }

        public static async Task BuildJsonAndMkDocs(DatabaseEngine dbEngine, string dbConnection, FileInfo dbSchemaFile, DirectoryInfo outputFolder, string language)
        {
            await Execute(async (cancellationToken, cancellationEvent) =>
            {
                var consoleWriter = ConsoleWriterFactory.CreateConsoleWriter(ProgressUnit.Count);
                var catalog = new Catalog("SqlDocs", Path.Combine(GetAppFolder(), "./L11n"), new CultureInfo(language));
                try
                {
                    consoleWriter.StartProgress("Generate database schema");

                    var provider = DatabaseProviderFactory.CreateDatabaseProvider(dbEngine, dbConnection);
                    var dbSchema = await DbSchemaFactory.CreateDbSchemaAsync(provider, cancellationToken);

                    consoleWriter.FinishProgress();

                    if (dbSchemaFile.Exists)
                    {
                        consoleWriter.StartProgress("Merge and update database schema file");

                        dbSchema = await DbSchemaFactory.LoadAndMergeDbSchemaAsync(dbSchemaFile, dbSchema, cancellationToken);
                        await dbSchema.SaveAsync(dbSchemaFile, cancellationToken);

                        consoleWriter.FinishProgress();
                    }
                    else
                    {
                        consoleWriter.StartProgress("Save database schema file");

                        await dbSchema.SaveAsync(dbSchemaFile, cancellationToken);

                        consoleWriter.FinishProgress();
                    }

                    consoleWriter.Success($"Schema file {dbSchemaFile.Name} successfully generated or updated");

                    consoleWriter.StartProgress("Generate MkDocs project");

                    var docsGenerator = DocsGeneratorFactory.CreateMkDocsGenerator(dbEngine, catalog);

                    await docsGenerator.GenerateAsync(dbSchema, outputFolder);

                    consoleWriter.FinishProgress();
                    consoleWriter.Success($"MkDocs project successfully generated or updated");

                }
                catch (Exception ex)
                {
                    consoleWriter.NewLine();
                    consoleWriter.Error($"Build failed. {ex.Message}");
                    Environment.ExitCode = 1;
                }
            });
        }

        public static async Task BuildMkDocs(DatabaseEngine dbEngine, FileInfo dbSchemaFile, DirectoryInfo outputFolder, string language)
        {
            await Execute(async (cancellationToken, cancellationEvent) =>
            {
                var ConsoleWriter = ConsoleWriterFactory.CreateConsoleWriter(ProgressUnit.Count);
                var catalog = new Catalog("SqlDocs", Path.Combine(GetAppFolder(), "./L11n"), new CultureInfo(language));
                try
                {
                    ConsoleWriter.StartProgress("Load database schema file");

                    var dbSchema = await DbSchemaFactory.LoadDbSchemaAsync(dbSchemaFile, cancellationToken);
                    await dbSchema.SaveAsync(dbSchemaFile, cancellationToken);

                    ConsoleWriter.FinishProgress();

                    ConsoleWriter.Success($"Schema file {dbSchemaFile.Name} successfully loaded");

                    ConsoleWriter.StartProgress("Generate MkDocs project");

                    var docsGenerator = DocsGeneratorFactory.CreateMkDocsGenerator(dbEngine, catalog);

                    await docsGenerator.GenerateAsync(dbSchema, outputFolder);

                    ConsoleWriter.FinishProgress();
                    ConsoleWriter.Success($"MkDocs project successfully generated or updated");

                }
                catch (Exception ex)
                {
                    ConsoleWriter.NewLine();
                    ConsoleWriter.Error($"Build failed. {ex.Message}");
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

        private static string GetAppFolder()
        {
            // Get the full location of the assembly
            string assemblyLocation = Environment.ProcessPath;

            // Get the folder that's in
            return Path.GetDirectoryName(assemblyLocation);
        }
    }
}
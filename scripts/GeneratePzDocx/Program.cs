using System.Text;
using System.Text.RegularExpressions;

namespace GeneratePzDocx;

internal static class Program
{
    private static readonly string RepoRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

    static int Main(string[] args)
    {
        var part = args.Length > 0 ? args[0] : "all";
        if (part is "1" or "part1")
            return GeneratePart1();
        if (part is "2" or "part2")
            return GeneratePart2();
        return GeneratePart1() | GeneratePart2();
    }

    static int GeneratePart1()
    {
        var mdPath = Path.Combine(RepoRoot, "Пояснительная_записка_часть_1.md");
        var outPath = Path.Combine(RepoRoot, "Пояснительная_записка_часть_1.docx");
        if (!File.Exists(mdPath))
        {
            Console.Error.WriteLine("Не найден файл: " + mdPath);
            return 1;
        }
        ConvertMarkdownFile(mdPath, outPath, includeAppendixA: false);
        Console.WriteLine("Создан файл: " + outPath);
        return 0;
    }

    static int GeneratePart2()
    {
        var mdPath = Path.Combine(RepoRoot, "Документация_ПЗ_раздел_2.5_заключение_и_приложения.md");
        var guidePath = Path.Combine(RepoRoot, "Руководство_системного_программиста.md");
        var outPath = Path.Combine(RepoRoot, "Пояснительная_записка_раздел_2.5_заключение_приложения.docx");

        if (!File.Exists(mdPath))
        {
            Console.Error.WriteLine("Не найден файл: " + mdPath);
            return 1;
        }

        ConvertMarkdownFile(mdPath, outPath, includeAppendixA: true);
        Console.WriteLine("Создан файл: " + outPath);
        return 0;
    }

    static void ConvertMarkdownFile(string mdPath, string outPath, bool includeAppendixA)
    {
        var guidePath = Path.Combine(RepoRoot, "Руководство_системного_программиста.md");
        var lines = File.ReadAllLines(mdPath, Encoding.UTF8);
        using var builder = new PzDocumentBuilder(outPath, "ПКТУ. ДП 8033.000ПЗ");

        var i = 0;
        var skipUntilAppendixB = false;

        while (i < lines.Length)
        {
            var line = lines[i];
            var trimmed = line.Trim();

            if (trimmed.StartsWith("# Пояснительная") || trimmed.StartsWith("## Раздел 2.5") ||
                trimmed.StartsWith("*Оформление") || trimmed == "---" && i < 10)
            {
                i++;
                continue;
            }

            if (trimmed == "---")
            {
                i++;
                continue;
            }

            if (trimmed.StartsWith("## ПРИЛОЖЕНИЕ А") && includeAppendixA)
            {
                builder.AddHeading("ПРИЛОЖЕНИЕ А", 1);
                builder.AddHeading("Руководство программиста", 2);
                if (File.Exists(guidePath))
                    ConvertGuideMarkdown(builder, File.ReadAllLines(guidePath, Encoding.UTF8));
                skipUntilAppendixB = true;
                i++;
                continue;
            }

            if (trimmed.StartsWith("## ПРИЛОЖЕНИЕ А") && !includeAppendixA)
            {
                i++;
                continue;
            }

            if (skipUntilAppendixB)
            {
                if (trimmed.StartsWith("## ПРИЛОЖЕНИЕ Б"))
                {
                    skipUntilAppendixB = false;
                }
                else
                {
                    i++;
                    continue;
                }
            }

            if (trimmed.StartsWith("## ПРИЛОЖЕНИЕ Г"))
            {
                builder.AddPageBreak();
                builder.AddHeading("ПРИЛОЖЕНИЕ Г", 1);
                builder.AddHeading("Исходный код программы", 2);
                AppendSourceCode(builder, RepoRoot);
                i++;
                while (i < lines.Length) i++;
                break;
            }

            ProcessMarkdownLine(builder, line, ref i, lines);
            i++;
        }

        builder.FinalizeDocument();
    }

    private static void ProcessMarkdownLine(PzDocumentBuilder b, string line, ref int index, string[] all)
    {
        var trimmed = line.Trim();
        if (string.IsNullOrWhiteSpace(trimmed)) return;

        if (trimmed.StartsWith("```"))
        {
            index++;
            var code = new List<string>();
            while (index < all.Length && !all[index].Trim().StartsWith("```"))
            {
                code.Add(all[index]);
                index++;
            }
            b.AddCodeBlock(code);
            return;
        }

        if (trimmed.StartsWith("|") && trimmed.Contains("|"))
        {
            if (trimmed.Contains("---")) return;
            var cells = trimmed.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            b.AddParagraphText(string.Join(" | ", cells), indent: false, leftIndent: 0);
            return;
        }

        if (trimmed.StartsWith("#### "))
        {
            b.AddHeading(trimmed[5..], 4);
            return;
        }
        if (trimmed.StartsWith("### "))
        {
            b.AddHeading(trimmed[4..], 3);
            return;
        }
        if (trimmed.StartsWith("## "))
        {
            b.AddHeading(trimmed[3..], 2);
            return;
        }

        if (trimmed.StartsWith("**Рисунок") && trimmed.Contains("**"))
        {
            b.AddFigureCaption(trimmed.Trim('*'));
            return;
        }

        if (trimmed.StartsWith("**Таблица") && trimmed.Contains("**"))
        {
            b.AddParagraphText(trimmed.Trim('*'), bold: true, center: true, indent: false);
            return;
        }

        if (trimmed.StartsWith("**") && trimmed.EndsWith("**") && trimmed.Length < 80 && !trimmed.Contains(':'))
        {
            b.AddParagraphText(trimmed.Trim('*'), bold: true, indent: false);
            return;
        }

        if (trimmed.StartsWith("*(Скриншот") || trimmed.StartsWith("*(") && trimmed.EndsWith(")*"))
            return;

        if (trimmed.StartsWith("*") && trimmed.EndsWith("*") && !trimmed.StartsWith("**"))
            return;

        if (Regex.IsMatch(trimmed, @"^\d+\.\s"))
        {
            b.AddNumberedSource(NormalizeBullets(trimmed));
            return;
        }

        if (trimmed.StartsWith("- ") || trimmed.StartsWith("− ") || Regex.IsMatch(trimmed, @"^\s+-\s"))
        {
            b.AddHyphenItem(NormalizeBullets(trimmed.Trim()));
            return;
        }

        if (trimmed.StartsWith("**") && trimmed.EndsWith("**"))
        {
            var inner = trimmed.Trim('*');
            if (inner.Contains(':') && inner.Length < 120)
            {
                b.AddParagraphText(inner, bold: true, indent: true);
                return;
            }
        }

        if (trimmed.StartsWith("1)") || trimmed.StartsWith("2)") || trimmed.StartsWith("3)") ||
            trimmed.StartsWith("4)") || trimmed.StartsWith("5)") || trimmed.StartsWith("6)") ||
            trimmed.StartsWith("7)") || trimmed.StartsWith("8)"))
        {
            b.AddParagraphText(NormalizeBullets(trimmed), indent: false, leftIndent: 709);
            return;
        }

        if (trimmed.StartsWith("`") && trimmed.EndsWith("`"))
        {
            b.AddHyphenItem(trimmed.Trim('`').Replace("—", "-"));
            return;
        }

        b.AddParagraphText(StripMarkdown(trimmed));
    }

    private static void ConvertGuideMarkdown(PzDocumentBuilder b, string[] lines)
    {
        b.AddHeading("Назначение и условия применения программы", 3);

        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (trimmed.StartsWith("# ПРИЛОЖЕНИЕ") || trimmed.StartsWith("# Руководство"))
                continue;
            if (trimmed == "---") continue;

            if (trimmed.StartsWith("### "))
            {
                b.AddHeading(trimmed[4..], 3);
                continue;
            }
            if (trimmed.StartsWith("## "))
            {
                b.AddHeading(trimmed[3..], 2);
                continue;
            }
            if (trimmed.StartsWith("```"))
                continue;
            if (trimmed.StartsWith("- ") || trimmed.StartsWith("− "))
            {
                b.AddHyphenItem(NormalizeBullets(trimmed));
                continue;
            }
            if (string.IsNullOrWhiteSpace(trimmed)) continue;
            if (trimmed.StartsWith("**") && trimmed.EndsWith("**"))
            {
                b.AddParagraphText(trimmed.Trim('*'), bold: true);
                continue;
            }
            b.AddParagraphText(StripMarkdown(trimmed));
        }

        AddAppendixAMessages(b);
    }

    private static void AddAppendixAMessages(PzDocumentBuilder b)
    {
        b.AddHeading("Сообщения", 3);
        b.AddParagraphText("При запуске программы могут возникнуть следующие сообщения:");
        b.AddHyphenItem("«Ошибка подключения к базе данных» — проверьте доступность SQL Server и параметры подключения (рисунок А.1);");
        b.AddFigureCaption("А.1 – Пример ошибки подключения к базе данных");
        b.AddHyphenItem("«Ошибка аутентификации» — неверный логин или пароль (рисунок А.2).");
        b.AddFigureCaption("А.2 – Пример ошибки аутентификации");

        b.AddHeading("Настройка программы", 3);
        b.AddParagraphText("Для настройки программы необходимо:");
        b.AddHyphenItem("развернуть приложение в отдельной директории на компьютере;");
        b.AddHyphenItem("настроить строку подключения в файле App.config, изменив параметры Server и Database, как показано на рисунке А.3.");
        b.AddFigureCaption("А.3 – Настройка строки подключения к базе данных");

        b.AddHeading("Проверка программы", 3);
        b.AddParagraphText("Запуск приложения приводит к отображению формы авторизации. При успешном подключении открывается главное окно программы с доступом к основным модулям.");
        b.AddParagraphText("Для дальнейшей проверки работоспособности необходимо выполнить тестовые операции по взаимодействию с базой данных, учёту инвентаря и формированию отчётов.");
    }

    private static void AppendSourceCode(PzDocumentBuilder b, string repo)
    {
        var files = new[]
        {
            Path.Combine(repo, "WindowsFormsApp1", "Program.cs"),
            Path.Combine(repo, "WindowsFormsApp1", "LoginForm.cs"),
            Path.Combine(repo, "WindowsFormsApp1", "Form1.cs"),
            Path.Combine(repo, "WindowsFormsApp1", "DbController.cs"),
            Path.Combine(repo, "WindowsFormsApp1", "Services", "ReportService.cs"),
        };

        foreach (var file in files)
        {
            if (!File.Exists(file)) continue;
            b.AddParagraphText(Path.GetFileName(file), bold: true, indent: false);
            var content = File.ReadAllLines(file, Encoding.UTF8);
            var take = Math.Min(content.Length, 120);
            b.AddCodeBlock(content.Take(take));
            if (content.Length > take)
                b.AddParagraphText("... (фрагмент; полный код в репозитории проекта)", italic: true);
            b.AddPageBreak();
        }
    }

    private static string NormalizeBullets(string s) =>
        s.Replace('\u2212', '-').Replace("•", "-").Trim();

    private static string StripMarkdown(string s)
    {
        s = Regex.Replace(s, @"\*\*(.+?)\*\*", "$1");
        s = s.Replace('`', ' ');
        return NormalizeBullets(s);
    }
}

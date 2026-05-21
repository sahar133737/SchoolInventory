using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace GeneratePzDocx;

internal sealed class PzDocumentBuilder : IDisposable
{
    private readonly WordprocessingDocument _doc;
    private readonly Body _body;
    private readonly string _docCode;
    public PzDocumentBuilder(string outputPath, string docCode = "ПКТУ.ДП8049.000ПЗ")
    {
        _docCode = docCode;
        _doc = WordprocessingDocument.Create(outputPath, WordprocessingDocumentType.Document);
        var mainPart = _doc.AddMainDocumentPart();
        mainPart.Document = new Document(new Body());
        _body = mainPart.Document.Body!;
        ApplyStyles(mainPart);
        _headerPart = mainPart.AddNewPart<HeaderPart>();
        _footerPart = mainPart.AddNewPart<FooterPart>();
        _mainPart = mainPart;
        BuildHeaderFooter();
    }

    private readonly MainDocumentPart _mainPart;
    private readonly HeaderPart _headerPart;
    private readonly FooterPart _footerPart;
    private SectionProperties? _sectPr;

    public void Dispose() => _doc.Dispose();

    public void FinalizeDocument()
    {
        _sectPr = new SectionProperties(
            new PageSize { Width = 11906, Height = 16838 },
            new PageMargin
            {
                Top = 1134,
                Right = 851,
                Bottom = 1134,
                Left = 1701,
                Header = 708,
                Footer = 708
            },
            new DocGrid { LinePitch = 360 },
            new HeaderReference { Type = HeaderFooterValues.Default, Id = _mainPart.GetIdOfPart(_headerPart) },
            new FooterReference { Type = HeaderFooterValues.Default, Id = _mainPart.GetIdOfPart(_footerPart) });
        _body.AppendChild(_sectPr);
        _mainPart.Document.Save();
    }

    public void AddSectionTitle(string text) =>
        AddParagraph(text, bold: true, center: true, caps: true, indent: false, spacingBefore: 240, spacingAfter: 240);

    public void AddHeading(string text, int level)
    {
        var bold = true;
        var center = false;
        var size = level <= 2 ? 28 : 28;
        if (text.StartsWith("ПРИЛОЖЕНИЕ", StringComparison.Ordinal) ||
            text is "ЗАКЛЮЧЕНИЕ" or "СПИСОК ИСПОЛЬЗОВАННЫХ ИСТОЧНИКОВ")
        {
            center = true;
            AddSectionTitle(text);
            return;
        }
        AddParagraph(text, bold, center, indent: false, sizeHalfPoints: size, spacingBefore: 180, spacingAfter: 120);
    }

    public void AddParagraphText(string text, bool bold = false, bool center = false, bool indent = true, bool italic = false, int leftIndent = 0)
    {
        if (string.IsNullOrWhiteSpace(text)) return;
        AddParagraph(Normalize(text), bold, center, indent: indent && !center && leftIndent == 0, italic: italic, leftIndent: leftIndent);
    }

    public void AddHyphenItem(string text)
    {
        var t = text.TrimStart();
        if (t.StartsWith("−")) t = "-" + t[1..];
        if (!t.StartsWith("-")) t = "- " + t;
        AddParagraph(t, indent: false, leftIndent: 709);
    }

    public void AddNumberedSource(string text) =>
        AddParagraph(text, indent: false, leftIndent: 0, hangingIndent: 709);

    public void AddFigureCaption(string caption)
    {
        var c = caption.Trim('*').Trim();
        c = c.Replace('–', '-').Replace('—', '-');
        var dash = c.IndexOf('-');
        if (dash > 0 && !c.Contains(" - "))
            c = c[..dash].TrimEnd() + " - " + c[(dash + 1)..].TrimStart('-', ' ');
        if (!c.StartsWith("Рисунок", StringComparison.OrdinalIgnoreCase))
            c = "Рисунок " + c;
        AddParagraph(c, center: true, indent: false, spacingBefore: 120, spacingAfter: 60);
        AddParagraph("[ Место для вставки рисунка ]", center: true, indent: false, italic: true, spacingAfter: 180);
    }

    public void AddCodeBlock(IEnumerable<string> lines)
    {
        foreach (var line in lines)
        {
            AddParagraph(line.Length == 0 ? " " : line, indent: false, font: "Consolas", sizeHalfPoints: 18, spacingAfter: 0);
        }
        AddParagraph("", indent: false);
    }

    public void AddPageBreak() => _body.AppendChild(new Paragraph(new Run(new Break { Type = BreakValues.Page })));

    private static string Normalize(string s) =>
        s.Replace('\u2212', '-').Replace("•", "-").Trim();

    private void AddParagraph(
        string text,
        bool bold = false,
        bool center = false,
        bool caps = false,
        bool indent = true,
        bool italic = false,
        int sizeHalfPoints = 28,
        int spacingBefore = 0,
        int spacingAfter = 0,
        int leftIndent = 0,
        int hangingIndent = 0,
        string font = "Times New Roman")
    {
        var p = new Paragraph();
        var pPr = new ParagraphProperties();
        pPr.AppendChild(new SpacingBetweenLines
        {
            Line = "360",
            LineRule = LineSpacingRuleValues.Auto,
            Before = spacingBefore.ToString(),
            After = spacingAfter.ToString()
        });
        if (center)
            pPr.AppendChild(new Justification { Val = JustificationValues.Center });
        else
            pPr.AppendChild(new Justification { Val = JustificationValues.Both });

        if (indent)
            pPr.AppendChild(new Indentation { FirstLine = "709" });
        else if (leftIndent > 0 || hangingIndent > 0)
            pPr.AppendChild(new Indentation
            {
                Left = leftIndent.ToString(),
                Hanging = hangingIndent > 0 ? hangingIndent.ToString() : null
            });

        p.AppendChild(pPr);
        var run = new Run();
        var rPr = new RunProperties();
        rPr.AppendChild(new RunFonts { Ascii = font, HighAnsi = font, ComplexScript = font });
        rPr.AppendChild(new FontSize { Val = sizeHalfPoints.ToString() });
        rPr.AppendChild(new FontSizeComplexScript { Val = sizeHalfPoints.ToString() });
        if (bold) rPr.AppendChild(new Bold());
        if (italic) rPr.AppendChild(new Italic());
        if (caps) rPr.AppendChild(new Caps());
        run.AppendChild(rPr);
        run.AppendChild(new Text(text) { Space = SpaceProcessingModeValues.Preserve });
        p.AppendChild(run);
        _body.AppendChild(p);
    }

    private static void ApplyStyles(MainDocumentPart mainPart)
    {
        var settingsPart = mainPart.AddNewPart<DocumentSettingsPart>();
        settingsPart.Settings = new Settings(
            new Compatibility(new CompatibilitySetting { Name = CompatSettingNameValues.CompatibilityMode, Val = "15" }));

        var stylesPart = mainPart.AddNewPart<StyleDefinitionsPart>();
        stylesPart.Styles = new Styles(
            new DocDefaults(
                new RunPropertiesDefault(new RunProperties(
                    new RunFonts { Ascii = "Times New Roman", HighAnsi = "Times New Roman" },
                    new FontSize { Val = "28" }))),
            new Style(
                new StyleName { Val = "Normal" },
                new PrimaryStyle(),
                new StyleRunProperties(
                    new RunFonts { Ascii = "Times New Roman", HighAnsi = "Times New Roman" },
                    new FontSize { Val = "28" })));
    }

    private void BuildHeaderFooter()
    {
        var header = new Header();
        var headerTable = new Table(
            new TableProperties(new TableWidth { Width = "5000", Type = TableWidthUnitValues.Pct }),
            new TableRow(
                Cell("Изм."),
                Cell("Лист"),
                Cell("№ докум."),
                Cell("Подпись"),
                Cell("Дата")),
            new TableRow(
                Cell(""),
                Cell("Лист"),
                Cell(_docCode),
                Cell(""),
                Cell("")));
        header.Append(headerTable);
        _headerPart.Header = header;

        var footer = new Footer();
        footer.Append(new Paragraph(
            new ParagraphProperties(new Justification { Val = JustificationValues.Center }),
            new Run(
                new RunProperties(
                    new RunFonts { Ascii = "Times New Roman", HighAnsi = "Times New Roman" },
                    new FontSize { Val = "24" }),
                new FieldChar { FieldCharType = FieldCharValues.Begin }),
            new Run(new FieldCode(" PAGE ")),
            new Run(new FieldChar { FieldCharType = FieldCharValues.Separate }),
            new Run(new Text("1")),
            new Run(new FieldChar { FieldCharType = FieldCharValues.End })));
        _footerPart.Footer = footer;
    }

    private static TableCell Cell(string text)
    {
        var run = new Run(
            new RunProperties(
                new RunFonts { Ascii = "Times New Roman", HighAnsi = "Times New Roman" },
                new FontSize { Val = "20" }),
            new Text(text));
        return new TableCell(new Paragraph(run));
    }
}

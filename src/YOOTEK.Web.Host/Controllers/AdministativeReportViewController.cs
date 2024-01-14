using Abp.Domain.Repositories;
using Abp.Web.Models;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Yootek.Configuration;
using Yootek.Controllers;
using Yootek.EntityDb;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Office2010.Word;
using Yootek.Authorization.Users;
using A = DocumentFormat.OpenXml.Drawing;
using DW = DocumentFormat.OpenXml.Drawing.Wordprocessing;
using PIC = DocumentFormat.OpenXml.Drawing.Pictures;
using Microsoft.AspNetCore.Http;

namespace Yootek.Web.Host.Controllers
{
    [Route("api/[controller]/[action]")]
    public class AdministativeReportViewController : YootekControllerBase
    {
        private const int MaxProfilePictureSize = 5242880; //5MB
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IRepository<Administrative, long> _administrativeRepos;
        private readonly IRepository<AdministrativeProperty, long> _administrativePropertyRepos;
        private readonly IRepository<TypeAdministrative, long> _typeAdministrativeRepos;
        private readonly IRepository<AdministrativeValue, long> _valueAdministrativeRepos;
        private readonly UserManager _userManager;
        private readonly IConfigurationRoot _appConfiguration;
        private string _rootPath = "";

        public AdministativeReportViewController(
            IWebHostEnvironment env,
            IHostingEnvironment hostingEnvironment,
            IRepository<Administrative, long> administrativeRepos,
            IRepository<AdministrativeProperty, long> administrativePropertyRepos,
            IRepository<TypeAdministrative, long> typeAdministrativeRepos,
            IRepository<AdministrativeValue, long> valueAdministrativeRepos,
            UserManager userManager
        )
        {
            _appConfiguration = env.GetAppConfiguration();
            _hostingEnvironment = hostingEnvironment;
            _administrativeRepos = administrativeRepos;
            _administrativePropertyRepos = administrativePropertyRepos;
            _typeAdministrativeRepos = typeAdministrativeRepos;
            _valueAdministrativeRepos = valueAdministrativeRepos;
            _userManager = userManager;
            _rootPath = _appConfiguration["App:ServerRootAddress"];
        }

        [HttpPost]
        [Route("uploadfile")]
        public async Task<string> UploadFile()
        {
            try
            {
                var request = await Request.ReadFormAsync();
                //Check input

                var ufile = request.Files[0];
                var profile = request["profile"].ToString();
                if (ufile != null && ufile.Length > 0)
                {
                    //var userId = _session.UserId;
                    var fileName = Path.GetFileName(ufile.FileName);
                    var pathName = "User" + AbpSession.UserId.ToString();
                    if (profile != null && profile != "")
                    {
                        pathName = pathName + @"/profileUser";
                    }

                    var folderPath = Path.Combine(_appConfiguration["PathStaticFile1"], @"images", pathName);
                    if (string.IsNullOrEmpty(_appConfiguration["PathStaticFile1"]))
                    {
                        folderPath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\images", pathName);
                    }

                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }

                    var filePath = Path.Combine(folderPath, fileName);
                    AppFileHelper.DeleteFilesInFolderIfExists(folderPath, fileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await ufile.CopyToAsync(fileStream);
                    }

                    var result = _appConfiguration["App:ServerRootAddress"] + @"images/" + pathName + @"/" + fileName;
                    var url = new Uri(result);

                    return url.AbsoluteUri;
                }

                return null;
            }
            catch (Exception ex)
            {
                Logger.Error(
                    $"{DateTime.Now.ToString("HH:mm:ss dd/MM/yyyy")} CheckFilePdfValid {ex.Message} {JsonConvert.SerializeObject(ex)}");
                return null;
            }
        }

        [HttpGet]
        [Route("loaddocument")]
        [DontWrapResult]
        public async Task<IActionResult> LoadDocument(int? tenantId, long id, string url)
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(tenantId))
                {
                    var user = await _userManager.GetUserByIdAsync(AbpSession.UserId.Value);
                    var userName = user.UserName;

                    var administratvie = _administrativeRepos.FirstOrDefault(id);
                    if (administratvie == null) return null;

                    var properites = _administrativePropertyRepos.GetAllList(x => x.TypeId == administratvie.ADTypeId);
                    if (properites == null) return null;
                    var values = _valueAdministrativeRepos.GetAllList(x => x.AdministrativeId == id);
                    if (values == null) return null;

                    byte[] byteArray = System.IO.File.ReadAllBytes(url);
                    using (MemoryStream mem = new MemoryStream())
                    {
                        mem.Write(byteArray, 0, (int)byteArray.Length);
                        string docText = null;
                        using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(mem, true))
                        {
                            using (StreamReader sr = new StreamReader(wordDoc.MainDocumentPart.GetStream()))
                            {
                                docText = sr.ReadToEnd();
                            }

                            foreach (var item in properites)
                            {
                                if (!string.IsNullOrEmpty(item.Key))
                                {
                                    var optionVal = values.Find(x => x.Key == item.Key);
                                    switch (item.Type)
                                    {
                                        case ADPropertyType.TABLE:
                                            List<AdministrativeProperty> listTable = new List<AdministrativeProperty>();
                                            List<AdministrativeProperty> tableChild =
                                                properites.FindAll(x => x.ParentId == item.Id);
                                            foreach (var child in tableChild)
                                            {
                                                listTable.Add(child);
                                            }

                                            List<AdministrativeValue> listValues = new List<AdministrativeValue>();
                                            while (true)
                                            {
                                                bool check = false;
                                                foreach (var currPor in listTable)
                                                {
                                                    AdministrativeValue valueChild =
                                                        values.Find(x => x.Key == currPor.Key);
                                                    if (valueChild != null)
                                                    {
                                                        listValues.Add(valueChild);
                                                        values.Remove(valueChild);
                                                        check = true;
                                                    }
                                                }

                                                if (check == false)
                                                {
                                                    break;
                                                }
                                            }

                                            var document = AddNewTableWord(item, wordDoc, listTable, listValues);
                                            // Regex regexText1 = new Regex(item.Key);
                                            // docText = regexText1.Replace(docText, "123");
                                            if (wordDoc.MainDocumentPart.Document.Body != null)
                                                foreach (Text element in wordDoc.MainDocumentPart.Document.Body
                                                             .Descendants<Text>())
                                                {
                                                    if (element.Text == item.Key)
                                                        element.Remove();
                                                }

                                            document.Save();
                                            break;
                                        case ADPropertyType.OPTION:
                                            AddCheckBoxAfterParagraph(wordDoc, optionVal);
                                            if (wordDoc.MainDocumentPart.Document.Body != null)
                                                foreach (Text element in wordDoc.MainDocumentPart.Document.Body
                                                             .Descendants<Text>())
                                                {
                                                    if (element.Text == item.Key)
                                                        element.Remove();
                                                }

                                            break;

                                        case ADPropertyType.CHECKNOTE:
                                            AddCommentOnFirstParagraph(wordDoc, userName, optionVal);
                                            break;
                                        case ADPropertyType.IMAGEFILE:
                                            InsertAPicture(wordDoc, optionVal);
                                            break;

                                        default:
                                            var value = "";
                                            var dt = values.Find(x => x.Key == item.Key);
                                            Regex regexText = new Regex(item.Key);
                                            if (dt != null && !string.IsNullOrEmpty(dt.Value)) value = dt.Value;
                                            docText = regexText.Replace(docText, value);
                                            break;
                                    }

                                    // var value = "";
                                    // var dt = values.Find(x => x.Key == item.Key);
                                    // Regex regexText = new Regex(item.Key);
                                    // if (dt != null && !string.IsNullOrEmpty(dt.Value)) value = dt.Value;
                                    // docText = regexText.Replace(docText, value);
                                    // break;
                                }
                            }

                            using (StreamWriter sw =
                                   new StreamWriter(wordDoc.MainDocumentPart.GetStream(FileMode.Create)))
                            {
                                sw.Write(docText);
                            }

                            wordDoc.Dispose();
                        }

                        Response.Headers.Append("Content-Type", "application/msword");
                        Response.Headers.Append("Content-Disposition", "attachment; filename=" + "test.docx");
                        Response.Headers.Remove("X-Frame-Options");
                        Response.Headers.Append("Access-Control-Expose-Headers", "Content-Disposition");
                        return File(mem.ToArray(), "application/msword");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Info(ex.Message);
                return null;
            }
        }


        [HttpGet]
        [Route("LoadDocx")]
        [DontWrapResult]
        public async Task<IActionResult> LoadDocx(int? tenantId, long id, string url)
        {
            try
            {
                using (MemoryStream mem = new MemoryStream())
                {
                    using (WordprocessingDocument wordDoc = WordprocessingDocument.Create(mem,
                               DocumentFormat.OpenXml.WordprocessingDocumentType.Document, true))
                    {
                        wordDoc.AddMainDocumentPart();
                        // siga a ordem
                        Document doc = new Document();
                        Body body = new Body();

                        // 1 paragrafo
                        DocumentFormat.OpenXml.Wordprocessing.Paragraph para =
                            new DocumentFormat.OpenXml.Wordprocessing.Paragraph();

                        ParagraphProperties paragraphProperties1 = new ParagraphProperties();
                        ParagraphStyleId paragraphStyleId1 = new ParagraphStyleId() { Val = "Normal" };
                        Justification justification1 = new Justification() { Val = JustificationValues.Center };
                        ParagraphMarkRunProperties paragraphMarkRunProperties1 = new ParagraphMarkRunProperties();

                        paragraphProperties1.Append(paragraphStyleId1);
                        paragraphProperties1.Append(justification1);
                        paragraphProperties1.Append(paragraphMarkRunProperties1);

                        Run run = new Run();
                        RunProperties runProperties1 = new RunProperties();

                        Text text = new Text() { Text = "The OpenXML SDK rocks!" };

                        // siga a ordem 
                        run.Append(runProperties1);
                        run.Append(text);
                        para.Append(paragraphProperties1);
                        para.Append(run);

                        // 2 paragrafo
                        DocumentFormat.OpenXml.Wordprocessing.Paragraph para2 =
                            new DocumentFormat.OpenXml.Wordprocessing.Paragraph();

                        ParagraphProperties paragraphProperties2 = new ParagraphProperties();
                        ParagraphStyleId paragraphStyleId2 = new ParagraphStyleId() { Val = "Normal" };
                        Justification justification2 = new Justification() { Val = JustificationValues.Start };
                        ParagraphMarkRunProperties paragraphMarkRunProperties2 = new ParagraphMarkRunProperties();

                        paragraphProperties2.Append(paragraphStyleId2);
                        paragraphProperties2.Append(justification2);
                        paragraphProperties2.Append(paragraphMarkRunProperties2);

                        Run run2 = new Run();
                        RunProperties runProperties3 = new RunProperties();
                        Text text2 = new Text();
                        text2.Text = "Teste aqui";

                        run2.AppendChild(new Break());
                        run2.AppendChild(new Text("Hello"));
                        run2.AppendChild(new Break());
                        run2.AppendChild(new Text("world"));

                        para2.Append(paragraphProperties2);
                        para2.Append(run2);

                        // todos os 2 paragrafos no main body
                        body.Append(para);
                        body.Append(para2);

                        doc.Append(body);

                        wordDoc.MainDocumentPart.Document = doc;

                        wordDoc.Dispose();
                    }

                    Response.Headers.Append("Content-Type", "application/msword");
                    Response.Headers.Append("Content-Disposition", "attachment; filename=" + "test.docx");
                    Response.Headers.Remove("X-Frame-Options");
                    Response.Headers.Append("Access-Control-Expose-Headers", "Content-Disposition");
                    return File(mem.ToArray(), "application/msword");
                }
            }
            catch (Exception e)
            {
                return null;
            }
        }

        [HttpGet]
        [Route("viewdocumnet")]
        [DontWrapResult]
        public async Task<IActionResult> Viewdocumnet(int? tenantId, long id)
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(tenantId))
                {
                    var administratvie = _administrativeRepos.FirstOrDefault(id);
                    if (administratvie == null) return null;
                    var typeAD = _typeAdministrativeRepos.FirstOrDefault(administratvie.ADTypeId);
                    if (typeAD == null || string.IsNullOrEmpty(typeAD.FileUrl)) return null;
                    byte[] byteArray = System.IO.File.ReadAllBytes(typeAD.FileUrl);

                    var properites = _administrativePropertyRepos.GetAllList(x => x.TypeId == administratvie.ADTypeId);
                    if (properites == null) return null;
                    var values = _valueAdministrativeRepos.GetAllList(x => x.AdministrativeId == id);
                    if (values == null) return null;
                    using (MemoryStream mem = new MemoryStream())
                    {
                        mem.Write(byteArray, 0, (int)byteArray.Length);
                        using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(mem, true))
                        {
                            var docText = "";
                            if (wordDoc.MainDocumentPart != null)
                            {
                                using (StreamReader sr = new StreamReader(wordDoc.MainDocumentPart.GetStream()))
                                {
                                    docText = sr.ReadToEnd();
                                }


                                foreach (var item in properites)
                                {
                                    if (!string.IsNullOrEmpty(item.Key))
                                    {
                                        var value = "";
                                        var dt = values.Find(x => x.Key == item.Key);
                                        Regex regexText = new Regex(item.Key);
                                        if (dt != null && !string.IsNullOrEmpty(dt.Value)) value = dt.Value;
                                        switch (item.Type)
                                        {
                                            case ADPropertyType.TABLE:
                                                docText = CreateTableWordDocument(item, docText, properites, values);
                                                break;
                                            case ADPropertyType.STRING:
                                            case ADPropertyType.NUMBER:
                                            case ADPropertyType.TIME:

                                                if (item.ParentId == null)
                                                {
                                                    docText = regexText.Replace(docText, value);
                                                }

                                                break;

                                            case ADPropertyType.DATETIME:
                                                if (item.ParentId == null)
                                                {
                                                    var date = DateTime.Parse(value);
                                                    value = date.Day + "/" + date.Month + "/" + date.Year;
                                                    docText = regexText.Replace(docText, value);
                                                }

                                                break;
                                            default:
                                                break;
                                        }
                                    }
                                }

                                using (StreamWriter sw =
                                       new StreamWriter(wordDoc.MainDocumentPart.GetStream(FileMode.Create)))
                                {
                                    sw.Write(docText);
                                }
                            }

                            wordDoc.Dispose();
                        }

                        Response.Headers.Append("Content-Type", "application/msword");
                        Response.Headers.Append("Content-Disposition", "attachment; filename=" + "test.docx");
                        Response.Headers.Remove("X-Frame-Options");
                        Response.Headers.Append("Access-Control-Expose-Headers", "Content-Disposition");
                        return File(mem.ToArray(), "application/msword");
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Info(e.Message);
                return null;
            }
        }


        #region Common

        private Document AddNewTableWord(AdministrativeProperty property, WordprocessingDocument document,
            List<AdministrativeProperty> properties, List<AdministrativeValue> values)
        {
            if (document.MainDocumentPart is null || document.MainDocumentPart.Document.Body is null)
            {
                throw new ArgumentNullException("MainDocumentPart and/or Body is null.");
            }

            MainDocumentPart mainPart = document.MainDocumentPart;

            Table table = new Table();

            TableProperties props = new TableProperties(
                new TableBorders(
                    new TopBorder
                    {
                        Val = new EnumValue<BorderValues>(BorderValues.Single),
                        Size = 12,
                    },
                    new BottomBorder
                    {
                        Val = new EnumValue<BorderValues>(BorderValues.Single),
                        Size = 12
                    },
                    new LeftBorder
                    {
                        Val = new EnumValue<BorderValues>(BorderValues.Single),
                        Size = 12
                    },
                    new RightBorder
                    {
                        Val = new EnumValue<BorderValues>(BorderValues.Single),
                        Size = 12
                    },
                    new InsideHorizontalBorder
                    {
                        Val = new EnumValue<BorderValues>(BorderValues.Single),
                        Size = 12
                    },
                    new InsideVerticalBorder
                    {
                        Val = new EnumValue<BorderValues>(BorderValues.Single),
                        Size = 12
                    }));

            table.AppendChild<TableProperties>(props);
            TableRow tableRow = new TableRow();
            TableCell tableCell1 = new TableCell();

            // Specify the width property of the table cell.  
            tableCell1.Append(new TableCellProperties(
                new TableCellWidth() { Type = TableWidthUnitValues.Dxa, Width = "24000" }));
            int count = 0;

            for (var i = 0; i <= values.Count; i++)
            {
                var tr = new TableRow();
                for (var j = 0; j < properties.Count; j++)
                {
                    var tc = new TableCell();
                    if (i == 0)
                    {
                        tc.Append(new Paragraph(new Run(new Text(properties[j].DisplayName))));
                        count--;
                    }

                    else
                    {
                        tc.Append(new Paragraph(new Run(new Text(values[count].Value))));
                    }

                    count++;

                    // Assume you want columns that are automatically sized.
                    tc.Append(new TableCellProperties(
                        new TableCellWidth { Type = TableWidthUnitValues.Dxa, Width = "12300" }));

                    tr.Append(tc);
                }

                table.Append(tr);
            }

            Paragraph existingParagraph = FindParagraphByText(mainPart, property.Key);

            // Insert the new paragraph and table after the existing paragraph
            if (existingParagraph != null)
            {
                // existingParagraph.InsertAfterSelf(newParagraph);
                existingParagraph.InsertAfterSelf(table);
            }

            // mainPart.Document.Body?.Append(table);
            return mainPart.Document;
        }

        private static Paragraph FindParagraphByText(MainDocumentPart mainPart, string searchText)
        {
            if (mainPart.Document.Body != null)
                foreach (var paragraph in mainPart.Document.Body.Elements<Paragraph>())
                {
                    if (paragraph.InnerText.Contains(searchText))
                    {
                        return paragraph;
                    }
                }

            return null;
        }

        static void AddCheckBoxAfterParagraph(WordprocessingDocument document,
            AdministrativeValue values)
        {
            MainDocumentPart mainPart = document.MainDocumentPart;

            // Find the existing paragraph
            if (mainPart != null)
            {
                if (mainPart.Document.Body != null)
                {
                    Paragraph existingParagraph = mainPart.Document.Body.Elements<Paragraph>().FirstOrDefault(p =>
                        p.InnerText.Contains(values.Key));

                    if (existingParagraph != null)
                    {
                        // Create a new paragraph with a run and a checkbox using content control
                        Paragraph newParagraph = new Paragraph(
                            new Run(
                                new SdtRun(
                                    new SdtProperties(
                                        new SdtAlias { Val = values.Value },
                                        new SdtContentCheckBox()
                                    ),
                                    new SdtContentRun(
                                        new Run(
                                            new RunProperties(
                                                new Bold(),
                                                new FontSize() { Val = "30" }
                                            ),
                                            new Text("Checkbox")
                                        )
                                    )
                                )
                            )
                        );

                        // Insert the new paragraph after the existing paragraph
                        existingParagraph.InsertAfterSelf(newParagraph);
                    }
                }

                // Save changes
                mainPart.Document.Save();
            }
        }

        static void AddCommentOnFirstParagraph(WordprocessingDocument document, string author,
            AdministrativeValue value)
        {
            // Use the file name and path passed in as an 
            // argument to open an existing Wordprocessing document. 
            if (document.MainDocumentPart is null /* || document.MainDocumentPart.WordprocessingCommentsPart is null*/)
            {
                throw new ArgumentNullException("MainDocumentPart and/or Body is null.");
            }

            WordprocessingCommentsPart wordprocessingCommentsPart =
                document.MainDocumentPart.WordprocessingCommentsPart ??
                document.MainDocumentPart.AddNewPart<WordprocessingCommentsPart>();

            // Locate the first paragraph in the document.
            Paragraph firstParagraph = document.MainDocumentPart.Document.Body.Elements<Paragraph>().FirstOrDefault(x =>
                x.InnerText.Contains("comment_1"));
            // Paragraph firstParagraph = document.MainDocumentPart.Document.Descendants<Paragraph>().First();
            wordprocessingCommentsPart.Comments = new Comments();
            string id = "0";

            // Verify that the document contains a 
            // WordProcessingCommentsPart part; if not, add a new one.
            if (document.MainDocumentPart.GetPartsOfType<WordprocessingCommentsPart>().Count() > 0)
            {
                if (wordprocessingCommentsPart.Comments.HasChildren)
                {
                    // Obtain an unused ID.
                    id = (wordprocessingCommentsPart.Comments.Descendants<Comment>().Select(e =>
                        {
                            if (e.Id != null && e.Id.Value != null)
                            {
                                return int.Parse(e.Id.Value);
                            }
                            else
                            {
                                throw new ArgumentNullException("Comment id and/or value are null.");
                            }
                        })
                        .Max() + 1).ToString();
                }
            }

            // Compose a new Comment and add it to the Comments part.
            Paragraph p = new Paragraph(new Run(new Text(value.Value)));
            Comment cmt =
                new Comment()
                {
                    Id = id,
                    Author = author,
                    Date = DateTime.Now
                };
            cmt.AppendChild(p);
            wordprocessingCommentsPart.Comments.AppendChild(cmt);
            wordprocessingCommentsPart.Comments.Save();

            // Specify the text range for the Comment. 
            // Insert the new CommentRangeStart before the first run of paragraph.
            firstParagraph.InsertBefore(new CommentRangeStart()
                { Id = id }, firstParagraph.GetFirstChild<Run>());

            // Insert the new CommentRangeEnd after last run of paragraph.
            var cmtEnd = firstParagraph.InsertAfter(new CommentRangeEnd()
                { Id = id }, firstParagraph.Elements<Run>().Last());

            // Compose a run with CommentReference and insert it.
            firstParagraph.InsertAfter(new Run(new CommentReference() { Id = id }), cmtEnd);
        }

        static void InsertAPicture(WordprocessingDocument wordprocessingDocument, AdministrativeValue value)
        {
            if (wordprocessingDocument.MainDocumentPart is null)
            {
                throw new ArgumentNullException("MainDocumentPart is null.");
            }

            MainDocumentPart mainPart = wordprocessingDocument.MainDocumentPart;

            ImagePart imagePart = mainPart.AddImagePart(ImagePartType.Jpeg);

            using (FileStream stream = new FileStream(value.Value, FileMode.Open))
            {
                imagePart.FeedData(stream);
            }

            AddImageToBody(wordprocessingDocument, mainPart.GetIdOfPart(imagePart));
        }

        static void AddImageToBody(WordprocessingDocument wordDoc, string relationshipId)
        {
            // Define the reference of the image.
            var element =
                new Drawing(
                    new DW.Inline(
                        new DW.Extent() { Cx = 990000L, Cy = 792000L },
                        new DW.EffectExtent()
                        {
                            LeftEdge = 0L,
                            TopEdge = 0L,
                            RightEdge = 0L,
                            BottomEdge = 0L
                        },
                        new DW.DocProperties()
                        {
                            Id = (UInt32Value)1U,
                            Name = "Picture 1"
                        },
                        new DW.NonVisualGraphicFrameDrawingProperties(
                            new A.GraphicFrameLocks() { NoChangeAspect = true }),
                        new A.Graphic(
                            new A.GraphicData(
                                    new PIC.Picture(
                                        new PIC.NonVisualPictureProperties(
                                            new PIC.NonVisualDrawingProperties()
                                            {
                                                Id = (UInt32Value)0U,
                                                Name = "New Bitmap Image.jpg"
                                            },
                                            new PIC.NonVisualPictureDrawingProperties()),
                                        new PIC.BlipFill(
                                            new A.Blip(
                                                new A.BlipExtensionList(
                                                    new A.BlipExtension()
                                                    {
                                                        Uri =
                                                            "{28A0092B-C50C-407E-A947-70E740481C1C}"
                                                    })
                                            )
                                            {
                                                Embed = relationshipId,
                                                CompressionState =
                                                    A.BlipCompressionValues.Print
                                            },
                                            new A.Stretch(
                                                new A.FillRectangle())),
                                        new PIC.ShapeProperties(
                                            new A.Transform2D(
                                                new A.Offset() { X = 0L, Y = 0L },
                                                new A.Extents() { Cx = 990000L, Cy = 792000L }),
                                            new A.PresetGeometry(
                                                    new A.AdjustValueList()
                                                )
                                                { Preset = A.ShapeTypeValues.Rectangle }))
                                )
                                { Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture" })
                    )
                    {
                        DistanceFromTop = (UInt32Value)0U,
                        DistanceFromBottom = (UInt32Value)0U,
                        DistanceFromLeft = (UInt32Value)0U,
                        DistanceFromRight = (UInt32Value)0U,
                        EditId = "50D07946"
                    });

            if (wordDoc.MainDocumentPart is null || wordDoc.MainDocumentPart.Document.Body is null)
            {
                throw new ArgumentNullException("MainDocumentPart and/or Body is null.");
            }

            // Append the reference to body, the element should be in a Run.
            // wordDoc.MainDocumentPart.Document.Body.AppendChild(new Paragraph(new Run(element)));
            Paragraph existingParagraph = FindParagraphByText(wordDoc.MainDocumentPart, "paragraph");

            // Insert the new paragraph and table after the existing paragraph
            if (existingParagraph != null)
            {
                // existingParagraph.InsertAfterSelf(newParagraph);
                existingParagraph.InsertAfterSelf(element);
            }

            // Save the document
            wordDoc.MainDocumentPart.Document.Save();
        }

        private string CreateTableWordDocument(AdministrativeProperty property, string doctext,
            List<AdministrativeProperty> properties, List<AdministrativeValue> values)
        {
            var columns = properties.FindAll(x => x.ParentId == property.Id);
            var count = 0;
            foreach (var item in columns)
            {
                var dts = values.FindAll(x => x.Key == item.Key);
                if (dts.Count > count) count = dts.Count;
                for (int i = 1; i < 6; i++)
                {
                    var value = "";
                    var dt = new AdministrativeValue();
                    if (dts.Count + 1 > i) dt = dts[i - 1];
                    Regex regexText = new Regex("#" + item.Key + i);
                    if (dt != null && !string.IsNullOrEmpty(dt.Value)) value = dt.Value;
                    doctext = regexText.Replace(doctext, value);
                }
            }

            for (int i = 1; i < 6; i++)
            {
                var value = "";
                if (i < count + 1) value = value + i;
                Regex regexText = new Regex("#STT" + i);
                doctext = regexText.Replace(doctext, value);
            }

            return doctext;
        }

        #endregion
    }
}
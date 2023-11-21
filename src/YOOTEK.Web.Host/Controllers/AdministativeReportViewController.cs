
using Abp.Domain.Repositories;
using Abp.Web.Models;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using IMAX.Configuration;
using IMAX.Controllers;
using IMAX.EntityDb;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IMAX.Web.Host.Controllers
{

    [Route("api/[controller]/[action]")]
    public class AdministativeReportViewController : IMAXControllerBase
    {


        private const int MaxProfilePictureSize = 5242880; //5MB
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IRepository<Administrative, long> _administrativeRepos;
        private readonly IRepository<AdministrativeProperty, long> _administrativePropertyRepos;
        private readonly IRepository<TypeAdministrative, long> _typeAdministrativeRepos;
        private readonly IRepository<AdministrativeValue, long> _valueAdministrativeRepos;
        private readonly IConfigurationRoot _appConfiguration;
        private string _rootPath = "";

        public AdministativeReportViewController(
            IWebHostEnvironment env,
            IHostingEnvironment hostingEnvironment,
            IRepository<Administrative, long> administrativeRepos,
            IRepository<AdministrativeProperty, long> administrativePropertyRepos,
            IRepository<TypeAdministrative, long> typeAdministrativeRepos,
            IRepository<AdministrativeValue, long> valueAdministrativeRepos

            )
        {
            _appConfiguration = env.GetAppConfiguration();
            _hostingEnvironment = hostingEnvironment;
            _administrativeRepos = administrativeRepos;
            _administrativePropertyRepos = administrativePropertyRepos;
            _typeAdministrativeRepos = typeAdministrativeRepos;
            _valueAdministrativeRepos = valueAdministrativeRepos;
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
                Logger.Error($"{DateTime.Now.ToString("HH:mm:ss dd/MM/yyyy")} CheckFilePdfValid {ex.Message} {JsonConvert.SerializeObject(ex)}");
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
                                    //switch (item.Type)
                                    //{
                                    //    case ADPropertyType.TABLE:
                                    //        docText = CreateTableWordDocument(item, docText, properites, values);
                                    //        break;
                                    //    default:
                                    //        var value = "";
                                    //        var dt = values.Find(x => x.Key == item.Key);
                                    //        Regex regexText = new Regex(item.Key);
                                    //        if (dt != null && !string.IsNullOrEmpty(dt.Value)) value = dt.Value;
                                    //        docText = regexText.Replace(docText, value);
                                    //        break;
                                    //}

                                    var value = "";
                                    var dt = values.Find(x => x.Key == item.Key);
                                    Regex regexText = new Regex(item.Key);
                                    if (dt != null && !string.IsNullOrEmpty(dt.Value)) value = dt.Value;
                                    docText = regexText.Replace(docText, value);
                                    break;

                                }
                            }

                            using (StreamWriter sw = new StreamWriter(wordDoc.MainDocumentPart.GetStream(FileMode.Create)))
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
                    using (WordprocessingDocument wordDoc = WordprocessingDocument.Create(mem, DocumentFormat.OpenXml.WordprocessingDocumentType.Document, true))
                    {
                        wordDoc.AddMainDocumentPart();
                        // siga a ordem
                        Document doc = new Document();
                        Body body = new Body();

                        // 1 paragrafo
                        DocumentFormat.OpenXml.Wordprocessing.Paragraph para = new DocumentFormat.OpenXml.Wordprocessing.Paragraph();

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
                        DocumentFormat.OpenXml.Wordprocessing.Paragraph para2 = new DocumentFormat.OpenXml.Wordprocessing.Paragraph();

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

                            using (StreamWriter sw = new StreamWriter(wordDoc.MainDocumentPart.GetStream(FileMode.Create)))
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
            catch (Exception e)
            {
                Logger.Info(e.Message);
                return null;
            }
        }


        #region Common

        private string CreateTableWordDocument(AdministrativeProperty property, string doctext, List<AdministrativeProperty> properties, List<AdministrativeValue> values)
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

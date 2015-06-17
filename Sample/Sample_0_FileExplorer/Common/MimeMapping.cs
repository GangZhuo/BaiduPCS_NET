using System;
using System.Collections;
using System.Text;

namespace FileExplorer
{
    public static class MimeMapping
    {
        private static Hashtable _mimeMappingTable;

        private static void AddMimeMapping(string extension, string MimeType)
        {
            MimeMapping._mimeMappingTable.Add(extension, MimeType);
        }

        public static string GetMimeMapping(string FileName)
        {
            string text = null;
            int num = FileName.LastIndexOf('.');
            if (0 < num && num > FileName.LastIndexOf('\\') && num > FileName.LastIndexOf('/'))
            {
                text = (string)MimeMapping._mimeMappingTable[FileName.Substring(num)];
            }
            if (text == null)
            {
                text = (string)MimeMapping._mimeMappingTable[".*"];
            }
            return text;
        }

        static MimeMapping()
        {
            MimeMapping._mimeMappingTable = new Hashtable(190, StringComparer.CurrentCultureIgnoreCase);
            MimeMapping.AddMimeMapping(".323", "text/h323");
            MimeMapping.AddMimeMapping(".ascx", "text/plain");
            MimeMapping.AddMimeMapping(".asp", "text/plain");
            MimeMapping.AddMimeMapping(".aspx", "text/plain");
            MimeMapping.AddMimeMapping(".asx", "video/x-ms-asf");
            MimeMapping.AddMimeMapping(".acx", "application/internet-property-stream");
            MimeMapping.AddMimeMapping(".ai", "application/postscript");
            MimeMapping.AddMimeMapping(".aif", "audio/x-aiff");
            MimeMapping.AddMimeMapping(".aiff", "audio/aiff");
            MimeMapping.AddMimeMapping(".axs", "application/olescript");
            MimeMapping.AddMimeMapping(".aifc", "audio/aiff");
            MimeMapping.AddMimeMapping(".asr", "video/x-ms-asf");
            MimeMapping.AddMimeMapping(".avi", "video/x-msvideo");
            MimeMapping.AddMimeMapping(".asf", "video/x-ms-asf");
            MimeMapping.AddMimeMapping(".au", "audio/basic");
            MimeMapping.AddMimeMapping(".application", "application/x-ms-application");
            MimeMapping.AddMimeMapping(".bin", "application/octet-stream");
            MimeMapping.AddMimeMapping(".bas", "text/plain");
            MimeMapping.AddMimeMapping(".bcpio", "application/x-bcpio");
            MimeMapping.AddMimeMapping(".bmp", "image/bmp");
            MimeMapping.AddMimeMapping(".cdf", "application/x-cdf");
            MimeMapping.AddMimeMapping(".cat", "application/vndms-pkiseccat");
            MimeMapping.AddMimeMapping(".crt", "application/x-x509-ca-cert");
            MimeMapping.AddMimeMapping(".c", "text/plain");
            MimeMapping.AddMimeMapping(".cc", "text/plain");
            MimeMapping.AddMimeMapping(".cpp", "text/plain");
            MimeMapping.AddMimeMapping(".cs", "text/plain");
            MimeMapping.AddMimeMapping(".css", "text/css");
            MimeMapping.AddMimeMapping(".cer", "application/x-x509-ca-cert");
            MimeMapping.AddMimeMapping(".crl", "application/pkix-crl");
            MimeMapping.AddMimeMapping(".cmx", "image/x-cmx");
            MimeMapping.AddMimeMapping(".csh", "application/x-csh");
            MimeMapping.AddMimeMapping(".cod", "image/cis-cod");
            MimeMapping.AddMimeMapping(".cpio", "application/x-cpio");
            MimeMapping.AddMimeMapping(".clp", "application/x-msclip");
            MimeMapping.AddMimeMapping(".crd", "application/x-mscardfile");
            MimeMapping.AddMimeMapping(".deploy", "application/octet-stream");
            MimeMapping.AddMimeMapping(".dll", "application/x-msdownload");
            MimeMapping.AddMimeMapping(".dot", "application/msword");
            MimeMapping.AddMimeMapping(".doc", "application/msword");
            MimeMapping.AddMimeMapping(".dvi", "application/x-dvi");
            MimeMapping.AddMimeMapping(".dir", "application/x-director");
            MimeMapping.AddMimeMapping(".dxr", "application/x-director");
            MimeMapping.AddMimeMapping(".der", "application/x-x509-ca-cert");
            MimeMapping.AddMimeMapping(".dib", "image/bmp");
            MimeMapping.AddMimeMapping(".dcr", "application/x-director");
            MimeMapping.AddMimeMapping(".disco", "text/xml");
            MimeMapping.AddMimeMapping(".exe", "application/octet-stream");
            MimeMapping.AddMimeMapping(".etx", "text/x-setext");
            MimeMapping.AddMimeMapping(".evy", "application/envoy");
            MimeMapping.AddMimeMapping(".eml", "message/rfc822");
            MimeMapping.AddMimeMapping(".eps", "application/postscript");
            MimeMapping.AddMimeMapping(".flr", "x-world/x-vrml");
            MimeMapping.AddMimeMapping(".fif", "application/fractals");
            MimeMapping.AddMimeMapping(".gtar", "application/x-gtar");
            MimeMapping.AddMimeMapping(".gif", "image/gif");
            MimeMapping.AddMimeMapping(".gz", "application/x-gzip");
            MimeMapping.AddMimeMapping(".hta", "application/hta");
            MimeMapping.AddMimeMapping(".htc", "text/x-component");
            MimeMapping.AddMimeMapping(".htt", "text/webviewhtml");
            MimeMapping.AddMimeMapping(".h", "text/plain");
            MimeMapping.AddMimeMapping(".hdf", "application/x-hdf");
            MimeMapping.AddMimeMapping(".hlp", "application/winhlp");
            MimeMapping.AddMimeMapping(".html", "text/html");
            MimeMapping.AddMimeMapping(".htm", "text/html");
            MimeMapping.AddMimeMapping(".hqx", "application/mac-binhex40");
            MimeMapping.AddMimeMapping(".isp", "application/x-internet-signup");
            MimeMapping.AddMimeMapping(".iii", "application/x-iphone");
            MimeMapping.AddMimeMapping(".ief", "image/ief");
            MimeMapping.AddMimeMapping(".ivf", "video/x-ivf");
            MimeMapping.AddMimeMapping(".ins", "application/x-internet-signup");
            MimeMapping.AddMimeMapping(".ico", "image/x-icon");
            MimeMapping.AddMimeMapping(".jpg", "image/jpeg");
            MimeMapping.AddMimeMapping(".jfif", "image/pjpeg");
            MimeMapping.AddMimeMapping(".jpe", "image/jpeg");
            MimeMapping.AddMimeMapping(".jpeg", "image/jpeg");
            MimeMapping.AddMimeMapping(".js", "text/x-javascript");
            MimeMapping.AddMimeMapping(".jsp", "text/plain");
            MimeMapping.AddMimeMapping(".log", "text/plain");
            MimeMapping.AddMimeMapping(".lsx", "video/x-la-asf");
            MimeMapping.AddMimeMapping(".latex", "application/x-latex");
            MimeMapping.AddMimeMapping(".lsf", "video/x-la-asf");
            MimeMapping.AddMimeMapping(".manifest", "application/x-ms-manifest");
            MimeMapping.AddMimeMapping(".mhtml", "message/rfc822");
            MimeMapping.AddMimeMapping(".mny", "application/x-msmoney");
            MimeMapping.AddMimeMapping(".mht", "message/rfc822");
            MimeMapping.AddMimeMapping(".mid", "audio/mid");
            MimeMapping.AddMimeMapping(".mpv2", "video/mpeg");
            MimeMapping.AddMimeMapping(".man", "application/x-troff-man");
            MimeMapping.AddMimeMapping(".mvb", "application/x-msmediaview");
            MimeMapping.AddMimeMapping(".mpeg", "video/mpeg");
            MimeMapping.AddMimeMapping(".m3u", "audio/x-mpegurl");
            MimeMapping.AddMimeMapping(".mdb", "application/x-msaccess");
            MimeMapping.AddMimeMapping(".mpp", "application/vnd.ms-project");
            MimeMapping.AddMimeMapping(".m1v", "video/mpeg");
            MimeMapping.AddMimeMapping(".mpa", "video/mpeg");
            MimeMapping.AddMimeMapping(".me", "application/x-troff-me");
            MimeMapping.AddMimeMapping(".m13", "application/x-msmediaview");
            MimeMapping.AddMimeMapping(".movie", "video/x-sgi-movie");
            MimeMapping.AddMimeMapping(".m14", "application/x-msmediaview");
            MimeMapping.AddMimeMapping(".mpe", "video/mpeg");
            MimeMapping.AddMimeMapping(".mp2", "video/mpeg");
            MimeMapping.AddMimeMapping(".mov", "video/quicktime");
            MimeMapping.AddMimeMapping(".mp3", "audio/mpeg");
            MimeMapping.AddMimeMapping(".mpg", "video/mpeg");
            MimeMapping.AddMimeMapping(".ms", "application/x-troff-ms");
            MimeMapping.AddMimeMapping(".nc", "application/x-netcdf");
            MimeMapping.AddMimeMapping(".nws", "message/rfc822");
            MimeMapping.AddMimeMapping(".oda", "application/oda");
            MimeMapping.AddMimeMapping(".ods", "application/oleobject");
            MimeMapping.AddMimeMapping(".pmc", "application/x-perfmon");
            MimeMapping.AddMimeMapping(".p7r", "application/x-pkcs7-certreqresp");
            MimeMapping.AddMimeMapping(".p7b", "application/x-pkcs7-certificates");
            MimeMapping.AddMimeMapping(".p7s", "application/pkcs7-signature");
            MimeMapping.AddMimeMapping(".pmw", "application/x-perfmon");
            MimeMapping.AddMimeMapping(".ps", "application/postscript");
            MimeMapping.AddMimeMapping(".p7c", "application/pkcs7-mime");
            MimeMapping.AddMimeMapping(".pbm", "image/x-portable-bitmap");
            MimeMapping.AddMimeMapping(".ppm", "image/x-portable-pixmap");
            MimeMapping.AddMimeMapping(".pub", "application/x-mspublisher");
            MimeMapping.AddMimeMapping(".pnm", "image/x-portable-anymap");
            MimeMapping.AddMimeMapping(".png", "image/png");
            MimeMapping.AddMimeMapping(".pml", "application/x-perfmon");
            MimeMapping.AddMimeMapping(".p10", "application/pkcs10");
            MimeMapping.AddMimeMapping(".pfx", "application/x-pkcs12");
            MimeMapping.AddMimeMapping(".p12", "application/x-pkcs12");
            MimeMapping.AddMimeMapping(".pdf", "application/pdf");
            MimeMapping.AddMimeMapping(".php", "text/plain");
            MimeMapping.AddMimeMapping(".pps", "application/vnd.ms-powerpoint");
            MimeMapping.AddMimeMapping(".p7m", "application/pkcs7-mime");
            MimeMapping.AddMimeMapping(".pko", "application/vndms-pkipko");
            MimeMapping.AddMimeMapping(".ppt", "application/vnd.ms-powerpoint");
            MimeMapping.AddMimeMapping(".pmr", "application/x-perfmon");
            MimeMapping.AddMimeMapping(".pma", "application/x-perfmon");
            MimeMapping.AddMimeMapping(".pot", "application/vnd.ms-powerpoint");
            MimeMapping.AddMimeMapping(".prf", "application/pics-rules");
            MimeMapping.AddMimeMapping(".pgm", "image/x-portable-graymap");
            MimeMapping.AddMimeMapping(".qt", "video/quicktime");
            MimeMapping.AddMimeMapping(".ra", "audio/x-pn-realaudio");
            MimeMapping.AddMimeMapping(".rgb", "image/x-rgb");
            MimeMapping.AddMimeMapping(".ram", "audio/x-pn-realaudio");
            MimeMapping.AddMimeMapping(".rmi", "audio/mid");
            MimeMapping.AddMimeMapping(".ras", "image/x-cmu-raster");
            MimeMapping.AddMimeMapping(".roff", "application/x-troff");
            MimeMapping.AddMimeMapping(".rtf", "application/rtf");
            MimeMapping.AddMimeMapping(".rtx", "text/richtext");
            MimeMapping.AddMimeMapping(".sv4crc", "application/x-sv4crc");
            MimeMapping.AddMimeMapping(".spc", "application/x-pkcs7-certificates");
            MimeMapping.AddMimeMapping(".setreg", "application/set-registration-initiation");
            MimeMapping.AddMimeMapping(".snd", "audio/basic");
            MimeMapping.AddMimeMapping(".stl", "application/vndms-pkistl");
            MimeMapping.AddMimeMapping(".setpay", "application/set-payment-initiation");
            MimeMapping.AddMimeMapping(".stm", "text/html");
            MimeMapping.AddMimeMapping(".shar", "application/x-shar");
            MimeMapping.AddMimeMapping(".sh", "application/x-sh");
            MimeMapping.AddMimeMapping(".sit", "application/x-stuffit");
            MimeMapping.AddMimeMapping(".spl", "application/futuresplash");
            MimeMapping.AddMimeMapping(".sct", "text/scriptlet");
            MimeMapping.AddMimeMapping(".scd", "application/x-msschedule");
            MimeMapping.AddMimeMapping(".sst", "application/vndms-pkicertstore");
            MimeMapping.AddMimeMapping(".src", "application/x-wais-source");
            MimeMapping.AddMimeMapping(".sv4cpio", "application/x-sv4cpio");
            MimeMapping.AddMimeMapping(".tex", "application/x-tex");
            MimeMapping.AddMimeMapping(".tgz", "application/x-compressed");
            MimeMapping.AddMimeMapping(".t", "application/x-troff");
            MimeMapping.AddMimeMapping(".tar", "application/x-tar");
            MimeMapping.AddMimeMapping(".tr", "application/x-troff");
            MimeMapping.AddMimeMapping(".tif", "image/tiff");
            MimeMapping.AddMimeMapping(".txt", "text/plain");
            MimeMapping.AddMimeMapping(".texinfo", "application/x-texinfo");
            MimeMapping.AddMimeMapping(".trm", "application/x-msterminal");
            MimeMapping.AddMimeMapping(".tiff", "image/tiff");
            MimeMapping.AddMimeMapping(".tcl", "application/x-tcl");
            MimeMapping.AddMimeMapping(".texi", "application/x-texinfo");
            MimeMapping.AddMimeMapping(".tsv", "text/tab-separated-values");
            MimeMapping.AddMimeMapping(".ustar", "application/x-ustar");
            MimeMapping.AddMimeMapping(".uls", "text/iuls");
            MimeMapping.AddMimeMapping(".vcf", "text/x-vcard");
            MimeMapping.AddMimeMapping(".wps", "application/vnd.ms-works");
            MimeMapping.AddMimeMapping(".wav", "audio/wav");
            MimeMapping.AddMimeMapping(".wrz", "x-world/x-vrml");
            MimeMapping.AddMimeMapping(".wri", "application/x-mswrite");
            MimeMapping.AddMimeMapping(".wks", "application/vnd.ms-works");
            MimeMapping.AddMimeMapping(".wmf", "application/x-msmetafile");
            MimeMapping.AddMimeMapping(".wcm", "application/vnd.ms-works");
            MimeMapping.AddMimeMapping(".wrl", "x-world/x-vrml");
            MimeMapping.AddMimeMapping(".wdb", "application/vnd.ms-works");
            MimeMapping.AddMimeMapping(".wsdl", "text/xml");
            MimeMapping.AddMimeMapping(".xap", "application/x-silverlight-app");
            MimeMapping.AddMimeMapping(".xml", "text/xml");
            MimeMapping.AddMimeMapping(".xlm", "application/vnd.ms-excel");
            MimeMapping.AddMimeMapping(".xaf", "x-world/x-vrml");
            MimeMapping.AddMimeMapping(".xla", "application/vnd.ms-excel");
            MimeMapping.AddMimeMapping(".xls", "application/vnd.ms-excel");
            MimeMapping.AddMimeMapping(".xof", "x-world/x-vrml");
            MimeMapping.AddMimeMapping(".xlt", "application/vnd.ms-excel");
            MimeMapping.AddMimeMapping(".xlc", "application/vnd.ms-excel");
            MimeMapping.AddMimeMapping(".xsl", "text/xml");
            MimeMapping.AddMimeMapping(".xbm", "image/x-xbitmap");
            MimeMapping.AddMimeMapping(".xlw", "application/vnd.ms-excel");
            MimeMapping.AddMimeMapping(".xpm", "image/x-xpixmap");
            MimeMapping.AddMimeMapping(".xwd", "image/x-xwindowdump");
            MimeMapping.AddMimeMapping(".xsd", "text/xml");
            MimeMapping.AddMimeMapping(".z", "application/x-compress");
            MimeMapping.AddMimeMapping(".zip", "application/x-zip-compressed");
            MimeMapping.AddMimeMapping(".*", "application/octet-stream");
        }

    }
}

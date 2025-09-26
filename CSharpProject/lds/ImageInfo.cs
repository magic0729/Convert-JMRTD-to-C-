using System.IO;

namespace org.jmrtd.lds
{
    public interface ImageInfo
    {
        public const string JPEG_MIME_TYPE = "image/jpeg";
        public const string JPEG2000_MIME_TYPE = "image/jp2";
        public const string WSQ_MIME_TYPE = "image/x-wsq";
        public const int TYPE_UNKNOWN = -1;
        public const int TYPE_PORTRAIT = 0;
        public const int TYPE_SIGNATURE_OR_MARK = 1;
        public const int TYPE_FINGER = 2;
        public const int TYPE_IRIS = 3;

        int GetType();
        string GetMimeType();
        int GetWidth();
        int GetHeight();
        long GetRecordLength();
        int GetImageLength();
        Stream GetImageInputStream();
    }
}

using System.Collections.Generic;

namespace IdentityService.Models
{
    public class Localized
    {
        public string en_US { get; set; }
        public string es_ES { get; set; }
    }

    public class PreferredLocale
    {
        public string country { get; set; }
        public string language { get; set; }
    }

    public class FirstName
    {
        public Localized localized { get; set; }
        public PreferredLocale preferredLocale { get; set; }
    }

    public class LastName
    {
        public Localized localized { get; set; }
        public PreferredLocale preferredLocale { get; set; }
    }

    public class Paging
    {
        public int count { get; set; }
        public int start { get; set; }
        public IList<object> links { get; set; }
    }

    public class StorageSize
    {
        public int width { get; set; }
        public int height { get; set; }
    }

    public class StorageAspectRatio
    {
        public double widthAspect { get; set; }
        public double heightAspect { get; set; }
        public string formatted { get; set; }
    }

    public class RawCodecSpec
    {
        public string name { get; set; }
        public string type { get; set; }
    }

    public class DisplaySize
    {
        public string uom { get; set; }
        public double width { get; set; }
        public double height { get; set; }
    }

    public class DisplayAspectRatio
    {
        public double widthAspect { get; set; }
        public double heightAspect { get; set; }
        public string formatted { get; set; }
    }

    public class ComLinkedinDigitalmediaMediaartifactStillImage
    {
        public StorageSize storageSize { get; set; }
        public StorageAspectRatio storageAspectRatio { get; set; }
        public string mediaType { get; set; }
        public RawCodecSpec rawCodecSpec { get; set; }
        public DisplaySize displaySize { get; set; }
        public DisplayAspectRatio displayAspectRatio { get; set; }
    }

    public class Identifier
    {
        public string identifier { get; set; }
        public string file { get; set; }
        public int index { get; set; }
        public string mediaType { get; set; }
        public string identifierType { get; set; }
        public int identifierExpiresInSeconds { get; set; }
    }

    public class Element
    {
        public string artifact { get; set; }
        public string authorizationMethod { get; set; }
        public IList<Identifier> identifiers { get; set; }
    }

    public class DisplayImage
    {
        public Paging paging { get; set; }
        public IList<Element> elements { get; set; }
    }

    public class ProfilePicture
    {
        public DisplayImage displayImage2 { get; set; }
    }

    public class UserInfo
    {
        public FirstName firstName { get; set; }
        public LastName lastName { get; set; }
        public ProfilePicture profilePicture { get; set; }
        public string id { get; set; }
        public string email { get; set; }
    }

    public class Handle
    {
        public string emailAddress { get; set; }
    }

    public class Element1
    {
        public Handle handle1 { get; set; }
        public string handle { get; set; }
    }

    public class UserEmail
    {
        public IList<Element1> elements { get; set; }
    }
}  

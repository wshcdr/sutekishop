using System;
using Suteki.Common.Extensions;
using Suteki.Shop.Controllers;

namespace Suteki.Shop.Views.Shared
{
    public partial class Site : System.Web.Mvc.ViewMasterPage
    {
        protected string SiteUrl
        {
            get
            {
                return GetController().BaseControllerService.SiteUrl;
            }
        }

        private IProvidesBaseService GetController()
        {
            if (ViewContext != null) return ViewContext.Controller as IProvidesBaseService;
            throw new ApplicationException("Controller does not implement IProvidesBaseService");
        }

        protected string Title
        {
            get
            {
                return GetController().BaseControllerService.ShopName;
            }
        }

        protected string Email
        {
            get
            {
                return GetController().BaseControllerService.EmailAddress;
            }
        }

        protected string Copyright
        {
            get
            {
                return GetController().BaseControllerService.Copyright;
            }
        }

        protected string PhoneNumber
        {
            get
            {
                return GetController().BaseControllerService.PhoneNumber;
            }
        }

        protected string RsdUrl
        {
            get
            {
                return "\"{0}rsd.xml\"".With(SiteUrl);
            }
        }

        protected string WlwManifestUrl
        {
            get
            {
                return "\"{0}wlwmanifest.xml\"".With(SiteUrl);
            }
        }

        protected string JQueryUrl
        {
            get
            {
                return "\"{0}Content/Script/jquery-1.2.6.min.js\"".With(SiteUrl);
            }
        }

        protected string GoogleTrackingCode
        {
            get
            {
                return "\"{0}\"".With(GetController().BaseControllerService.GoogleTrackingCode);
            }
        }

        protected string SiteCss
        {
            get
            {
                return "\"{0}Content/{1}\"".With(SiteUrl, GetController().BaseControllerService.SiteCss);
            }
        }

    }
}

using Microsoft.AspNetCore.Components;
using System.Globalization;
using Radzen;

namespace ProcureHub.BlazorApp.Components.Shared
{
    public enum GravatarStyle
    {
        Initials,
        Color,
        FourOhFour,
        MysteryPerson,
        Identicon,
        MonsterId,
        Wavatar,
        Retro,
        RoboHash,
        Blank
    }

    public partial class Gravatar : RadzenComponent
    {
        /// <summary>
        /// Gets or sets the email address used to fetch the Gravatar image.
        /// The email is hashed (MD5) and used to query Gravatar.com for the associated profile image.
        /// </summary>
        /// <value>The email address.</value>
        [Parameter]
        public string? Email { get; set; }

        [Parameter]
        public string? Initials { get; set; }

        /// <summary>
        /// Gets or sets the alternate text describing the avatar for accessibility.
        /// This text is read by screen readers and displayed if the image fails to load.
        /// </summary>
        /// <value>The image alternate text. Default is "gravatar".</value>
        [Parameter]
        public string AlternateText { get; set; } = "gravatar";

        [Parameter]
        public GravatarStyle GravatarStyle { get; set; } = GravatarStyle.Identicon;

        /// <summary>
        /// Gets or sets the size of the avatar image in pixels (both width and height).
        /// Gravatar provides square images at various sizes.
        /// </summary>
        /// <value>The avatar size in pixels. Default is 36.</value>
        [Parameter]
        public int Size { get; set; } = 36;

        /// <summary>
        /// Gets gravatar URL.
        /// </summary>
        private string Url
        {
            get
            {
                var md5Email = MD5.Calculate(System.Text.Encoding.ASCII.GetBytes(Email ?? ""));
                var defaultStyle = MapGravatarStyle(GravatarStyle);
                return $"https://secure.gravatar.com/avatar/{md5Email}?d={defaultStyle}&s={Size}&initials={Initials ?? ""}";
            }
        }

        string GetAlternateText()
        {
            if (Attributes != null && Attributes.TryGetValue("alt", out var @alt) && !string.IsNullOrEmpty(Convert.ToString(@alt, CultureInfo.InvariantCulture)))
            {
                return $"{AlternateText} {@alt}";
            }

            return AlternateText;
        }

        /// <inheritdoc />
        protected override string GetComponentCssClass()
        {
            return "rz-gravatar";
        }

        private static string MapGravatarStyle(GravatarStyle style)
        {
            return style switch
            {
                GravatarStyle.Initials => "initials",
                GravatarStyle.Color => "color",
                GravatarStyle.FourOhFour => "404",
                GravatarStyle.MysteryPerson => "mp",
                GravatarStyle.Identicon => "identicon",
                GravatarStyle.MonsterId => "monsterid",
                GravatarStyle.Wavatar => "wavatar",
                GravatarStyle.Retro => "retro",
                GravatarStyle.RoboHash => "robohash",
                _ => "blank"
            };
        }
    }
}

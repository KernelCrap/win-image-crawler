using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.IO;
using System.Linq;

using Crawler.Interfaces;

namespace Crawler.Implementation
{
	public class ImageCrawlerProcesserRules: ICrawlerProcesserRules<Image>
	{
		// This "bag" adds as a simple collection to keep track of which urls we have
		// already processed. As the name of the class implies, this is thread-safe.
		private readonly ConcurrentBag<string> _processed = new ConcurrentBag<string>();

		// Allowed file extensions, valid urls must have one of these extensions.
		public ConcurrentBag<string> AllowedExtensions { get; private set; }

		// The minimum size of the images saved can be defined.
		public int MinWidth { get; set; }
		public int MinHeight { get; set; }

		public ImageCrawlerProcesserRules()
		{
			AllowedExtensions = new ConcurrentBag<string>();
		}

		public bool IsValidUri(Uri uri)
		{
			// Simple validation of the passed object.
			if (uri == null)
				return false;

			// If the uri contains a fragment, it is not valid.
			// We could also just check for #.
			if (!String.IsNullOrWhiteSpace(uri.Fragment))
				return false;

			// Since we do not wish to keep processing the
			// same pages over and over, we keep track of
			// all pages that have been processed.
			if (_processed.Contains(uri.AbsoluteUri))
				return false;

			// The extension must be within the allowed images extensions.
			// If no extensions are defined, nothing is valid.
			if (!AllowedExtensions.Contains(Path.GetExtension(uri.AbsoluteUri)))
				return false;

			// If we reach this point, the url is validated,
			// and has not been processed before. Therefore we
			// add it to the collection of processed urls.
			_processed.Add(uri.AbsoluteUri);

			// The url is valid at this point.
			return true;
		}

		public bool IsValid(Image input)
		{
			// Simple validation of the passed object.
			if (input == null)
				return false;

			return input.Width > MinWidth && input.Height > MinHeight;
		}
	}
}
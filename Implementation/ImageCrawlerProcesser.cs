using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;

using Crawler.Interfaces;
using HtmlAgilityPack;
using NLog;

namespace Crawler.Implementation
{
	public class ImageCrawlerProcesser : ICrawlerProcesser<HtmlDocument>
	{
		// The constructor requires rules to be defined, the rules are used
		// by the processer to validate urls and decide to process them or not.
		private readonly ICrawlerProcesserRules<Image> _rules;

		// For logging purposes we use NLog.
		private static readonly Logger Log = LogManager.GetCurrentClassLogger();

		// The folder where the images are saved.
		private readonly string _imageFolder = String.Empty;

		public ImageCrawlerProcesser(ICrawlerProcesserRules<Image> rules, string imageFolder)
		{
			_imageFolder = imageFolder;
			_rules = rules;
		}

		private void DownloadImage(Uri url, string destination)
		{
			Log.Debug("[Thread #{0}]: DownloadImage: {1}", Thread.CurrentThread.ManagedThreadId, url);

			// Attempt to download the file.
			try
			{
				// Create a new client and download the file.
				using (var client = new WebClient())
				{
					// Download the image as a byte array.
					var data = client.DownloadData(url);

					// Create a new Image object from the data received.
					using (var stream = new MemoryStream(data))
					using (var image = Image.FromStream(stream))
					{
						// Make sure the image lives up to the our expectations.
						if (!_rules.IsValid(image))
							return;

						// Extract the filename from the url.
						var fileName = Path.GetFileName(url.AbsoluteUri);
						if (fileName == null)
							return;

						// Make sure the directory exsits,
						// otherwise create it.
						if (!Directory.Exists(destination))
							Directory.CreateDirectory(destination);

						// Create the full path to the locationwhere the
						// file is to be saved.
						var fullPath = Path.Combine(destination, fileName);

						// Save the image to the location,
						// don't do anything if it already exists.
						if (!File.Exists(fullPath))
							image.Save(fullPath);
					}
				}
			}
			catch (WebException ex)
			{
				Log.ErrorException(ex.Message, ex);
			}
			catch (ArgumentException ex)
			{
				Log.ErrorException(ex.Message, ex);
			}
			catch (ExternalException ex)
			{
				Log.ErrorException(ex.Message, ex);
			}
		}

		public void Process(Uri uri, HtmlDocument document)
		{
			// Simple validation of the passed object.
			if (uri == null || document == null)
				return;

			Log.Debug("[Thread #{0}]: Process: {1}", Thread.CurrentThread.ManagedThreadId, uri.AbsoluteUri);

			// Since images can be both links and actual img tags, we simply
			// get both and process them.
			var links = document.DocumentNode.SelectNodes("//a[@href]");
			var images = document.DocumentNode.SelectNodes("//img[@src]");

			// The folder where the images are saved should be based
			// on their absolute uris, so http://example.com/fun/funny.png
			// should go the the folder: _imageFolder\example.com\fun\funny.png
			var folder = Path.Combine(_imageFolder, uri.Host);
			folder = Path.Combine(folder, uri.LocalPath.Substring(1).Replace("/", @"\"));

			// Here we loop through and processes all image tags.
			if (images != null)
			{
				// Each image should be download if the url is valid.
				foreach (var image in images)
				{
					try
					{
						// Get the full path to the image.
						var path = new Uri(uri, image.Attributes["src"].Value);

						// Just continue the loop if the url is not valid.
						if (!_rules.IsValidUri(path))
							continue;

						// Call the DownloadImage method if the url is valid.
						DownloadImage(path, folder);

						Log.Debug("[Thread #{0}]: Done: {1}", Thread.CurrentThread.ManagedThreadId, uri.AbsoluteUri);
					}
					catch (UriFormatException ex)
					{
						Log.ErrorException(ex.Message, ex);
					}
				}
			}

			// Since each link could be an image (validated by the rules),
			// we also loop through each of the links.
			if (links != null)
			{
				// Loop through all links on the page.
				foreach (var link in links)
				{
					try
					{
						// Get the actual path of the link.
						var path = new Uri(uri, link.Attributes["href"].Value);

						// Call the DownloadImage method if the url is valid.
						if (_rules.IsValidUri(path))
							DownloadImage(path, folder);
					}
					catch (UriFormatException ex)
					{
						Log.ErrorException(ex.Message, ex);
					}
				}
			}
		}
	}
}
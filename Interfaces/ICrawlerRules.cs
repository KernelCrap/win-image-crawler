using System;

namespace Crawler.Interfaces
{
	public interface ICrawlerRules<in T>
	{
		bool IsValidUri(Uri uri);
		bool IsValidPage(T document);
	}
}
================
Image downloader
================
A small program to download all images on a given website written in C#.
Feel free to modify it to fit your needs.

Requirements
============
The following packages are required:

* .NET Framework 4.5
* `HtmlAgilityPack <http://nuget.org/packages/HtmlAgilityPack>`_
* `NLog <http://nuget.org/packages/NLog>`_

What does it do?
================
As it is, the program is given a place to start, then it will do the following:

1. Download the given page and parses it.
2. Loops through all images on the page.
3. Download & save the images.
4. Loop through all links and then goes to step 2. for each of those.

How to use it?
==============

1. Load up the project in Visual Studio (the project is for Visual Studio 2012).
2. Use NuGet to download and install the required packages.
3. Modify *Program.cs* to fit your needs.
4. Compile and run the program.

Example
=======
A website (example.com) with the following html::

    <!doctype html>
    <html lang="en">
    <head>
        <title>Example.com</title>
        <meta charset="utf-8" />	
    </head>
    <body>
        <img src="pictures/picture1.jpg" />
        <img src="pictures/picture2.jpg" />
        <a href="pictures/picture3.jpg">Open image</a>
    </body>
    </html>

Would result in the program downloading the following files::

    C:\example.com\pictures\picture1.jpg
    C:\example.com\pictures\picture2.jpg
    C:\example.com\pictures\picture3.jpg

The *example.com* folder in the example, will be placed in the folder given in the program.

#region Copyright & License Information
/*
 * Copyright 2007-2016 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System.Drawing;
using System.IO;
using System.Linq;
using OpenRA.FileSystem;
using OpenRA.Primitives;

namespace OpenRA.Graphics
{
	public interface ISpriteLoader
	{
		bool TryParseSprite(Stream s, out ISpriteFrame[] frames);
	}

	public interface ISpriteFrame
	{
		/// <summary>
		/// Size of the frame's `Data`.
		/// </summary>
		Size Size { get; }

		/// <summary>
		/// Size of the entire frame including the frame's `Size`.
		/// Think of this like a picture frame.
		/// </summary>
		Size FrameSize { get; }

		float2 Offset { get; }
		byte[] Data { get; }
		bool DisableExportPadding { get; }
	}

	public class SpriteCache
	{
		public readonly SheetBuilder SheetBuilder;
		readonly Cache<string, Sprite[]> sprites;

		public SpriteCache(IReadOnlyFileSystem fileSystem, ISpriteLoader[] loaders, SheetBuilder sheetBuilder)
		{
			SheetBuilder = sheetBuilder;

			sprites = new Cache<string, Sprite[]>(filename => SpriteLoader.GetSprites(fileSystem, filename, loaders, sheetBuilder));
		}

		public Sprite[] this[string filename] { get { return sprites[filename]; } }
	}

	public class FrameCache
	{
		readonly Cache<string, ISpriteFrame[]> frames;

		public FrameCache(IReadOnlyFileSystem fileSystem, ISpriteLoader[] loaders)
		{
			frames = new Cache<string, ISpriteFrame[]>(filename => SpriteLoader.GetFrames(fileSystem, filename, loaders));
		}

		public ISpriteFrame[] this[string filename] { get { return frames[filename]; } }
	}

	public static class SpriteLoader
	{
		public static Sprite[] GetSprites(IReadOnlyFileSystem fileSystem, string filename, ISpriteLoader[] loaders, SheetBuilder sheetBuilder)
		{
			return GetFrames(fileSystem, filename, loaders).Select(a => sheetBuilder.Add(a)).ToArray();
		}

		public static ISpriteFrame[] GetFrames(IReadOnlyFileSystem fileSystem, string filename, ISpriteLoader[] loaders)
		{
			using (var stream = fileSystem.Open(filename))
			{
				var spriteFrames = GetFrames(stream, loaders);
				if (spriteFrames == null)
					throw new InvalidDataException(filename + " is not a valid sprite file!");

				return spriteFrames;
			}
		}

		public static ISpriteFrame[] GetFrames(Stream stream, ISpriteLoader[] loaders)
		{
			ISpriteFrame[] frames;
			foreach (var loader in loaders)
				if (loader.TryParseSprite(stream, out frames))
					return frames;

			return null;
		}
	}
}

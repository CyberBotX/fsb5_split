using System;
using System.IO;
using System.Linq;
using System.Text;

#nullable enable

namespace fsb5_split
{
	static class Program
	{
		class FSB5Header
		{
			public byte[] id { get; set; } = null!;
			public int version { get; set; }
			public int numSamples { get; set; }
			public int shdrSize { get; set; }
			public int nameSize { get; set; }
			public int dataSize { get; set; }
			public uint mode { get; set; }
			public byte[]? extra { get; set; }
			public byte[] zero { get; set; } = null!;
			public byte[] hash { get; set; } = null!;
			public byte[] dummy { get; set; } = null!;
		}

		static Func<BinaryReader, uint> fr32 = null!;
		static Func<BinaryReader, ulong> fr64 = null!;

		static uint fri32(BinaryReader br) => br.ReadUInt32();
		static ulong fri64(BinaryReader br) => br.ReadUInt64();

		static uint frb32(BinaryReader br) => BitConverter.ToUInt32(br.ReadBytes(4).Reverse().ToArray(), 0);
		static ulong frb64(BinaryReader br) => BitConverter.ToUInt64(br.ReadBytes(8).Reverse().ToArray(), 0);

		static int CheckSignEndian(BinaryReader br)
		{
			br.BaseStream.Position = 0;
			byte[] sign = br.ReadBytes(4);
			br.BaseStream.Position = 0;

			if (sign[0] == 'F' && sign[1] == 'S' && sign[2] == 'B')
			{
				fr32 = fri32;
				fr64 = fri64;
				return sign[3];
			}
			else if (sign[1] == 'B' && sign[2] == 'S' && sign[3] == 'F')
			{
				fr32 = frb32;
				fr64 = frb64;
				return sign[0];
			}
			else
				return -1;
		}

		static int ReadFSB5Header(BinaryReader br, FSB5Header fsb5Header)
		{
			long oldPosition = br.BaseStream.Position;

			fsb5Header.id = br.ReadBytes(4);
			fsb5Header.version = (int)fr32(br);
			fsb5Header.numSamples = (int)fr32(br);
			fsb5Header.shdrSize = (int)fr32(br);
			fsb5Header.nameSize = (int)fr32(br);
			fsb5Header.dataSize = (int)fr32(br);
			fsb5Header.mode = fr32(br);
			fsb5Header.extra = fsb5Header.version == 0 ? br.ReadBytes(4) : null;
			fsb5Header.zero = br.ReadBytes(8);
			fsb5Header.hash = br.ReadBytes(16);
			fsb5Header.dummy = br.ReadBytes(8);

			return (int)(br.BaseStream.Position - oldPosition);
		}

		static readonly Func<ulong, uint> GET_FSB5_OFFSET = X => (uint)((X >> 7) & 0x7FFFFFF) << 5;

		static void InternalCopyStream(this Stream input, Stream? output, int bufferSize, int bytesToWrite)
		{
			if (output is null)
				throw new ArgumentException("Output Stream was null", nameof(output));
			if (!input.CanRead && !input.CanWrite)
				throw new ObjectDisposedException("input", "Stream closed");
			if (!output.CanRead && !output.CanWrite)
				throw new ObjectDisposedException("output", "Stream closed");
			if (!input.CanRead)
				throw new NotSupportedException("Unreadable input Stream");
			if (!output.CanWrite)
				throw new NotSupportedException("Unwriteable output Stream");
			byte[] buffer = new byte[bufferSize];
			int totalWritten = 0;
			while (true)
			{
				int toRead = Math.Min(bufferSize, bytesToWrite - totalWritten);
				if (toRead <= 0)
					break;
				int read = input.Read(buffer, 0, toRead);
				if (read <= 0)
					break;
				output.Write(buffer, 0, read);
				totalWritten += read;
			}
		}

		public static void CopyStream(this Stream? input, Stream? output, int bytesToWrite)
		{
			if (input is null)
				throw new ArgumentException("Input Stream was null", nameof(input));
			input.InternalCopyStream(output, 81920, bytesToWrite);
		}

		static void Main(string[] args)
		{
			if (args.Length < 1)
			{
				Console.WriteLine("Syntax: {0} <fsb file> [<output directory>]", typeof(Program).Assembly.Location);
				Console.WriteLine("NOTE: If the output directory is not given, the new FSB files will be written");
				Console.WriteLine("      to the same directory as the given FSB file.");
				return;
			}

			string baseName = Path.GetFileNameWithoutExtension(args[0]);
			string outputDirectory = args.Length == 2 ? args[1] : Path.Combine(Path.GetDirectoryName(args[0])!, baseName);

			if (!Directory.Exists(outputDirectory))
				Directory.CreateDirectory(outputDirectory);

			using var br = new BinaryReader(File.OpenRead(args[0]));
			int sign = CheckSignEndian(br);
			if (sign < 0)
			{
				Console.WriteLine("Invalid file detected.");
				return;
			}
			if (sign != '5')
			{
				Console.WriteLine("This tool is designed for FSB5 files only.");
				return;
			}

			var fsb5Header = new FSB5Header();
			int fhSize = ReadFSB5Header(br, fsb5Header);
			uint nameOffset = (uint)(fhSize + fsb5Header.shdrSize);
			uint fileOffset = (uint)(nameOffset + fsb5Header.nameSize);
			uint baseOffset = fileOffset;

			bool pcmEndian = (fsb5Header.zero[4] & 1) == 1;

			for (int i = 0; i < fsb5Header.numSamples; ++i)
			{
				ulong offset = fr64(br);

				uint type = (uint)(offset & 0x7F);
				byte[] shdrData = BitConverter.GetBytes(offset & ~(0x7FFFFFFUL << 5)); // This will basically make it so the offset becomes 0 for the new split file
				offset = GET_FSB5_OFFSET(offset); // This is the offset into the file section

				while ((type & 1) == 1)
				{
					uint t32 = fr32(br);
					shdrData = shdrData.Concat(BitConverter.GetBytes(t32)).ToArray();
					type = t32 & 1;
					int len = (int)((t32 & 0xFFFFFF) >> 1);
					shdrData = shdrData.Concat(br.ReadBytes(len)).ToArray();
				}

				long currOffset = br.BaseStream.Position;
				uint size = br.BaseStream.Position < nameOffset && i + 1 != fsb5Header.numSamples ? GET_FSB5_OFFSET(fr64(br)) + baseOffset : (uint)br.BaseStream.Length;
				br.BaseStream.Position = currOffset;
				fileOffset = (uint)(baseOffset + offset);
				size -= fileOffset;

				string name = $"{baseName}_{i:X8}";
				string internalName = "";
				if (fsb5Header.nameSize != 0)
				{
					currOffset = br.BaseStream.Position;
					br.BaseStream.Position = nameOffset + i * 4;
					br.BaseStream.Position = nameOffset + fr32(br);
					do
					{
						byte c = br.ReadByte();
						if (c == 0)
							break;
						name += (char)c;
					} while (true);
					internalName = name;
					br.BaseStream.Position = currOffset;
				}

				Console.Write($"Processing {name}...");

				string outputFilename = Path.Combine(outputDirectory, $"{name}.fsb");

				currOffset = br.BaseStream.Position;
				br.BaseStream.Position = fileOffset;
				// Get file
				using (var bw = new BinaryWriter(File.Create(outputFilename)))
				{
					bw.Write(Encoding.ASCII.GetBytes("FSB5"));
					bw.Write(fsb5Header.version);
					bw.Write(1);
					bw.Write(shdrData.Length);
					int fullNameSize = (int)Math.Ceiling((internalName.Length + 5) / 16.0) * 16;
					bw.Write(fullNameSize);
					bw.Write(size);
					bw.Write(fsb5Header.mode);
					if (fsb5Header.version == 0)
						bw.Write(fsb5Header.extra!);
					bw.Write(fsb5Header.zero);
					bw.Write(fsb5Header.hash);
					bw.Write(fsb5Header.dummy);
					bw.Write(shdrData);
					bw.Write(4);
					bw.Write(Encoding.ASCII.GetBytes(internalName));
					bw.Write((byte)0);
					for (int j = internalName.Length + 5; j < fullNameSize; ++j)
						bw.Write((byte)0);
					br.BaseStream.CopyStream(bw.BaseStream, (int)size);
				}
				br.BaseStream.Position = currOffset;

#if FMOD
				// Also use the FMOD API to output to a WAV file
				var result = FMOD.Factory.System_Create(out var system);

				string outputWAV = Path.Combine(outputDirectory, $"{name}.wav");

				result = system.setOutput(FMOD.OUTPUTTYPE.WAVWRITER);

				result = system.init(32, FMOD.INITFLAGS.NORMAL, System.Runtime.InteropServices.Marshal.StringToHGlobalAnsi(outputWAV));

				result = system.createSound(args[0], FMOD.MODE._2D | FMOD.MODE.LOOP_OFF | FMOD.MODE.CREATESTREAM, out var sound);

				result = sound.getSubSound(i, out var subsound);

				result = system.playSound(subsound, null!, false, out var channel);

				do
				{
					result = system.update();
					if (channel is not null)
					{
						result = channel.isPlaying(out bool playing);
						if (!playing)
							break;
					}
				} while (true);

				result = sound.release();
				result = system.close();
				result = system.release();
#endif

				Console.WriteLine(" DONE!");
			}
		}
	}
}

#nullable restore

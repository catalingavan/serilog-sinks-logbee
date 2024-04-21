﻿using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Serilog.Sinks.LogBee.AspNetCore
{
    internal class MirrorStreamDecorator : Stream
    {
        private readonly Stream _decorated;
        private readonly MemoryStream _mirrorStream;
        private readonly Encoding _encoding;

        public MirrorStreamDecorator(Stream decorated, Encoding? encoding = null)
        {
            _decorated = decorated ?? throw new ArgumentNullException(nameof(decorated));
            _encoding = encoding ?? Encoding.UTF8;
            _mirrorStream = new MemoryStream();
        }

        public override bool CanRead => _decorated.CanRead;
        public override bool CanSeek => _decorated.CanSeek;
        public override bool CanWrite => _decorated.CanWrite;
        public override long Length => _decorated.Length;

        public override long Position
        {
            get => _decorated.Position;
            set => _decorated.Position = value;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _decorated.Read(buffer, offset, count);
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return _decorated.ReadAsync(buffer, offset, count, cancellationToken);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _decorated.Write(buffer, offset, count);

            if (_mirrorStream?.CanWrite == true)
                _mirrorStream.Write(buffer, offset, count);
        }

        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            await _decorated.WriteAsync(buffer, offset, count, cancellationToken);

            if (_mirrorStream?.CanWrite == true)
                _mirrorStream.Write(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _decorated.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            _decorated.SetLength(value);

            if (_mirrorStream?.CanWrite == true)
                _mirrorStream.SetLength(value);
        }

        public override void Flush()
        {
            _decorated.Flush();
        }

        public override Task FlushAsync(CancellationToken cancellationToken)
        {
            return _decorated.FlushAsync(cancellationToken);
        }

        public override void Close()
        {
            _decorated.Close();
        }

        public MemoryStream MirrorStream => _mirrorStream;
        public Encoding Encoding => _encoding;
    }
}

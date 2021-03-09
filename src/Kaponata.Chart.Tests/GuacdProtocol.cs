// <copyright file="GuacdProtocol.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Kaponata.Chart.Tests
{
    /// <summary>
    /// A very basic client which speaks the guacamole daemon protocol. Used by the guacd integration tests,
    /// to ensure the service is up and running.
    /// </summary>
    /// <seealso href="https://guacamole.apache.org/doc/gug/guacamole-protocol.html"/>
    public class GuacdProtocol
    {
        private readonly Stream stream;

        /// <summary>
        /// Initializes a new instance of the <see cref="GuacdProtocol"/> class.
        /// </summary>
        /// <param name="stream">
        /// A <see cref="Stream"/> which represents a connection to the guacd service.
        /// </param>
        public GuacdProtocol(Stream stream)
        {
            this.stream = stream ?? throw new ArgumentNullException(nameof(stream));
        }

        /// <summary>
        /// Asynchronously sends an instruction to the remote end.
        /// </summary>
        /// <param name="opcode">
        /// The opcode of the instruction.
        /// </param>
        /// <param name="args">
        /// The arguments for the instruction.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task SendInstructionAsync(string opcode, string[] args, CancellationToken cancellationToken)
        {
            StringBuilder instructionBuilder = new StringBuilder();

            Encode(instructionBuilder, opcode);

            foreach (var arg in args)
            {
                instructionBuilder.Append(",");
                Encode(instructionBuilder, arg);
            }

            instructionBuilder.Append(";");

            var data = Encoding.UTF8.GetBytes(instructionBuilder.ToString());
            return this.stream.WriteAsync(data, 0, data.Length, cancellationToken);
        }

        /// <summary>
        /// Asynchronously reads an instruction.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// </param>
        /// <returns>
        /// A <see cref="string"/> array which contains the opcode and arguments.
        /// </returns>
        public async Task<string[]> ReadInstructionAsync(CancellationToken cancellationToken)
        {
            byte[] buffer = new byte[1024];

            int read = await this.stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false);
            string command = Encoding.UTF8.GetString(buffer, 0, read);
            string[] args = command.Split(new char[] { ',', ';' });

            for (int i = 0; i < args.Length - 1; i++)
            {
                var separator = args[i].IndexOf('.');
                var length = int.Parse(args[i].Substring(0, separator));
                var value = args[i].Substring(separator + 1);

                Debug.Assert(length == value.Length, "Incorrect length");

                args[i] = value;
            }

            return args;
        }

        private static void Encode(StringBuilder instructionBuilder, string command)
        {
            instructionBuilder.Append(command.Length);
            instructionBuilder.Append(".");
            instructionBuilder.Append(command);
        }
    }
}

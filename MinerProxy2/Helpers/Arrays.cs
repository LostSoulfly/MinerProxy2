using System;
using System.Collections.Generic;

namespace MinerProxy2.Helpers
{
    public static class Arrays
    {
        public static int GetBytesIndex(this byte[] bytes, byte[] pattern, int startIndex = 0)
        {
            for (int i = startIndex; i < bytes.Length; i++)
            {
                if (bytes.Length - i < pattern.Length)
                    return -1;

                if (pattern[0] != bytes[i])
                    continue;

                for (int j = 0; j < pattern.Length; j++)
                {
                    if (bytes[i + j] != pattern[j])
                        break;

                    if (j == pattern.Length - 1)
                        return i;
                }
            }

            return -1;
        }

        public static byte[] JoinArrays(byte[] a, int aLength, byte[] b, int bLength)
        {
            byte[] output = new byte[aLength + bLength];
            for (int i = 0; i < aLength; i++)
                output[i] = a[i];
            for (int j = 0; j < bLength; j++)
                output[aLength + j] = b[j];
            return output;
        }

        public static List<byte[]> ProcessBuffer(ref byte[] existingBuffer, ref int existingBufferLength,
            byte[] newBuffer, int newBufferLength, byte[] seperator, int BUFFER_SIZE = 4096)
        {
            byte[] buffer;
            byte[] tempBuffer;
            List<byte[]> bufferList = new List<byte[]>();
            int lastIndex = 0;
            int index = 0;

            //if our existing buffer is empty, just parse the new buffer, otherwise combine them and check everything.
            if (existingBufferLength > 0)
            {
                buffer = JoinArrays(existingBuffer, existingBufferLength, newBuffer, newBufferLength);
            }
            else
            {
                if (newBuffer.Length != newBufferLength)
                {
                    buffer = new byte[newBufferLength];
                    Array.Copy(newBuffer, buffer, newBufferLength);
                }
                else
                {
                    buffer = newBuffer;
                }
            }
            // Get the first position of the seperator
            index = GetBytesIndex(buffer, seperator) + seperator.Length;

            // if we have no seperators, just build the exisingBuffer and return an empty list.
            if (index == 0)
            {
                // If the total buffer would exceed the buffer size, just return it empty and clear the buffers.
                //Sure, we'll lose some data, but we probably already lost parts of a packet somewhere anyway.
                if (buffer.Length > BUFFER_SIZE)
                {
                    existingBuffer = new byte[BUFFER_SIZE];
                    existingBufferLength = 0;
                    return bufferList;
                }

                //If our new buffer will fit,
                //clear the existingBuffer and copy in our new buffer, updating the buffer length
                existingBuffer = new byte[BUFFER_SIZE];
                existingBufferLength = buffer.Length;
                Array.Copy(buffer, existingBuffer, buffer.Length);
                Console.WriteLine("no index, empty buffer list, save buffer");
                // and return the empty bufferList because we have no complete strings
                return bufferList;
            }

            // If we're here, then we have at least one full string to add to the list
            do
            {
                tempBuffer = new byte[index - lastIndex];                               // We get the current string's length based on the current index and last index
                Array.Copy(buffer, lastIndex, tempBuffer, 0, tempBuffer.Length);        // Copy the full string into the tempBuffer
                bufferList.Add(tempBuffer);                                             // Add the full string to the bufferList for returning
                lastIndex = index;                                                      // update the lastIndex

                index = GetBytesIndex(buffer, seperator, lastIndex) + seperator.Length; // Search the buffer again starting at the lastIndex, and repeat above if found
            } while (index > 0 || index == buffer.Length);

            //If we have leftover buffer, add it to the existing buffer
            if (lastIndex != buffer.Length)
            {
                Console.WriteLine("leftover buffer, adding to unused");
                existingBuffer = new byte[BUFFER_SIZE];
                existingBufferLength = buffer.Length - lastIndex;
                Array.Copy(buffer, lastIndex, existingBuffer, 0, buffer.Length - lastIndex);
            }
            else
            {
                //If we have no more buffer, reset the existingBuffer and it's length
                existingBuffer = new byte[BUFFER_SIZE];
                existingBufferLength = 0;
            }

            //and return the current bufferList containing our full strings of bytes
            return bufferList;
        }
    }
}
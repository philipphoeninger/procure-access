// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace DAL.ProcureAccess.Exceptions;

public class CustomException : Exception
{
    public CustomException() { }
    public CustomException(string message) : base(message) { }
    public CustomException(string message, Exception innerException) : base(message, innerException) { }
}

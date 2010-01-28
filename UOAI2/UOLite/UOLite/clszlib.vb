'********************************************************************************
'general purpose zlib.dll wrapper for compressing text, files, byte arrays, etc.
'
'December 12, 2004
'
'MS VB.NET 2003 (v7.1.3088), option explicit=on, option strict=on
'
'Kevin Pisarsky (http://www.pisarsky.com) (kevin@pisarsky.com)
'
'for usage see the demo application that should have been packed with this code
'
'********************************************************************************
Imports System.Runtime.InteropServices

Public Class clszlib

    'the version of zlib used is zlibwapi.dll, v1.2.2.0, which was downloaded from the
    'official zlib page at http://www.gzip.org/zlib/
    '
    'this dll is the one compiled for windows9x/nt/2000/xp usage and is win32 compatible.
    'The dll was used without compiling from source, and without any modifications.
    '

    'class level constants

    Public Const Z_NO_FLUSH As Integer = 0
    'partial flush deprecated, use Z_SYNC_FLUSH instead 
    Public Const Z_PARTIAL_FLUSH As Integer = 1
    Public Const Z_SYNC_FLUSH As Integer = 2
    Public Const Z_FULL_FLUSH As Integer = 3
    Public Const Z_FINISH As Integer = 4
    'allowed flush values ; see deflate() for details */

    Public Const Z_OK As Integer = 0
    Public Const Z_STREAM_END As Integer = 1
    Public Const Z_NEED_DICT As Integer = 2
    Public Const Z_ERRNO As Integer = -1
    Public Const Z_STREAM_ERROR As Integer = -2
    Public Const Z_DATA_ERROR As Integer = -3
    Public Const Z_MEM_ERROR As Integer = -4
    Public Const Z_BUF_ERROR As Integer = -5
    Public Const Z_VERSION_ERROR As Integer = -6

    'return codes for the compression/decompression functions. Negative
    'values are errors, positive values are used for special but normal events.
    Public Const Z_NO_COMPRESSION As Integer = 0
    Public Const Z_BEST_SPEED As Integer = 1
    Public Const Z_BEST_COMPRESSION As Integer = 9
    Public Const Z_DEFAULT_COMPRESSION As Integer = -1

    'compression levels
    Public Const Z_FILTERED As Integer = 1
    Public Const Z_HUFFMAN_ONLY As Integer = 2
    Public Const Z_DEFAULT_STRATEGY As Integer = 0

    'compression strategy 
    Public Const Z_BINARY As Integer = 0
    Public Const Z_ASCII As Integer = 1
    Public Const Z_UNKNOWN As Integer = 2

    'possible values of the data_type field 
    Public Const Z_DEFLATED As Integer = 8
    'the deflate compression method (the only one supported in this version)
    'for initializing zalloc, zfree, opaque 
    Public Const Z_NULL As Integer = 0

    'class level variables

    'this is the file handle type
    Private gzFile As Long

    'this is the buffer used by intCompressByteArray to store
    'an array of compressed bytes - it is public so that the calling
    'application can have access to it if necessary
    Public m_arbytCompressionBuffer As Byte()

    'this is the buffer used by intDeCompressByteArray to store
    'an array of decompressed bytes - it is public so that the calling
    'application can have access to it if necessary
    Public m_arbytDeCompressionBuffer As Byte()

    'the original size of the data being compressed, whether it is
    'a buffer, file size or whatever. this number is set when you compress
    'something, and used when you uncompress it
    Private m_lngOriginalSize As Long

    'this is the zlib function for compressing a byte array
    <DllImport("zlibwapi.dll", EntryPoint:="compress")> Private Shared Function CompressByteArray(ByVal dest As Byte(), ByRef destLen As Integer, ByVal src As Byte(), ByVal srcLen As Integer) As Integer
        'leave function empty - DLLImport attribute forwards calls to CompressByteArray to compress in zlib.dLL
    End Function

    'this is the zlib function for decompressing a byte array
    <DllImport("zlibwapi.dll", EntryPoint:="uncompress")> Private Shared Function UncompressByteArray(ByVal dest As Byte(), ByRef destLen As Integer, ByVal src As Byte(), ByVal srcLen As Integer) As Integer
        'leave function empty - DLLImport attribute forwards calls to UnCompressByteArray to Uncompress in zlib.dLL
    End Function
    'zlib function for opening a file for writing
    <DllImport("zlibwapi.dll", EntryPoint:="gzopen")> Private Shared Function OpenWrite(ByVal strPath As String, ByVal strMode As String) As Long
        'leave empty
    End Function
    Public Property lngOriginalSize() As Long
        'this property holds the original size of data being compressed. it is necessary to
        'keep the original size of the data in order to uncompress it. whenever possible this
        'property is automatically set by the compression procedures; it may be necessary for
        'your application to set the property if the application is shut down or this class is
        'deallocated between compression and decompression procedures.
        Get
            lngOriginalSize = m_lngOriginalSize
        End Get
        Set(ByVal lngValue As Long)
            m_lngOriginalSize = lngValue
        End Set
    End Property
    'compress a byte array into a buffer, return value:
    ' the size of the compressed buffer if the operation was successful
    ' return -1 if the operation was unsuccessful but no specific error is provided
    ' return -4 if there was not enough memory to complete the operation
    ' return -5 if there was not enough room in the output buffer (intBufferSize not large enough)
    Public Function intCompressByteArray(ByRef arbytData() As Byte, Optional ByRef TempBuffer() As Byte = Nothing) As Integer
        'parameters: arbytData is a one dimensional array of bytes

        Dim intResult As Integer 'result of the call to zlibwapi.dll
        Dim intBufferSize As Integer 'size of the uncompressed byte array being passed in

        'set the original size of the data, this is used when the data is to be
        'uncompressed
        m_lngOriginalSize = UBound(arbytData) + 1

        'set the size of the buffer for the data to be compressed into; the buffer size 
        'must be at least 0.1% larger than the uncompressed data, plus 12 bytes. after
        'the function is successful, intBufferSize will be the actual size of the
        'compressed buffer
        intBufferSize = CInt(m_lngOriginalSize)
        intBufferSize = CInt(intBufferSize + (intBufferSize * 0.015) + 12)

        'redimension the compression buffer to the correct size
        'for this operation
        ReDim m_arbytCompressionBuffer(intBufferSize)

        'call the zlibwapi.dll function "compress" to actually compress the bytes
        intResult = CompressByteArray(m_arbytCompressionBuffer, intBufferSize, arbytData, UBound(arbytData) + 1)

        'set the function result
        Select Case intResult
            Case Z_OK : Return intBufferSize 'size of compressed buffer
            Case Z_MEM_ERROR : Return -4 'not enough memory
            Case Z_BUF_ERROR : Return -5 'output buffer (m_arbytCompressionBuffer) too small
            Case Else
                Return -1 'unknown error
        End Select

    End Function 'intCompressByteArray
    'decompress a byte array into an uncompressed buffer, return value:
    ' the size of the compressed buffer if the operation was successful
    ' return -1 if the operation was unsuccessful but no specific error is provided
    ' return -4 if there was not enough memory to complete the operation
    ' return -5 if there was not enough room in the output buffer (intBufferSize not large enough)
    Public Function intDeCompressByteArray(ByRef arbytData() As Byte) As Integer
        'parameters: arbytData is a one dimensional array of bytes that have been compressed
        ' note:  you need to set the value of m_lngOriginalSize before calling this
        '        function, it requires the original size of the data before compression

        Dim intResult As Integer 'result of the call to zlibwapi.dll
        Dim intBufferSize As Integer 'size of the uncompressed byte array being passed in

        'set the size of the buffer for the data to be uncompressed into; the buffer size 
        'must be at least the uncompressed data, plus 12 bytes. after
        'the function is successful, intBufferSize will be the actual size of the
        'uncompressed buffer
        intBufferSize = CInt(m_lngOriginalSize)

        'redimension the decompression buffer to the correct size
        'for this operation
        ReDim m_arbytDeCompressionBuffer(intBufferSize)

        'call the zlibwapi.dll function "compress" to actually compress the bytes
        intResult = UncompressByteArray(m_arbytDeCompressionBuffer, intBufferSize, arbytData, UBound(arbytData) + 1)

        'set the function result
        Select Case intResult
            Case Z_OK : Return intBufferSize 'size unof compressed buffer
            Case Z_MEM_ERROR : Return -4 'not enough memory
            Case Z_BUF_ERROR : Return -5 'output buffer (m_arbytCompressionBuffer) too small
            Case Else
                Return -1 'unknown error
        End Select

    End Function 'intDeCompressByteArray
End Class

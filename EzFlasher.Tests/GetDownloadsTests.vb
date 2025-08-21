Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class GetDownloadsTests
    <TestMethod>
    Public Sub GetFileName_WithQuery_ReturnsFileName()
        Dim getter As New GetDownloads()
        Dim result As String = getter.getFileName("http://example.com/file.zip?retrieve_file=1")
        Assert.AreEqual("file.zip", result)
    End Sub

    <TestMethod>
    Public Sub GetFileName_WithoutQuery_ReturnsFileName()
        Dim getter As New GetDownloads()
        Dim result As String = getter.getFileName("http://example.com/file.zip")
        Assert.AreEqual("file.zip", result)
    End Sub

    <TestMethod>
    Public Sub GetFileName_WithPath_ReturnsFileName()
        Dim getter As New GetDownloads()
        Dim result As String = getter.getFileName("http://example.com/path/to/file.zip")
        Assert.AreEqual("file.zip", result)
    End Sub
End Class

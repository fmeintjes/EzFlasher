Imports System.Net
Imports System.IO

'@author Weidi Zhang (http://github.com/ebildude123)
'@license GPL v3 (https://www.gnu.org/copyleft/gpl.html)
'Copyright (C) 2014

Public Class GetDownloads
    Private Function makeHTTPRequest(ByVal url As String, Optional ByVal returnUrlOnly As Boolean = False)
        Dim httpReq As HttpWebRequest = DirectCast(WebRequest.Create(url), HttpWebRequest)
        httpReq.KeepAlive = True
        httpReq.AllowAutoRedirect = True
        httpReq.ContentType = "application/x-www-form-urlencoded"
        httpReq.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/32.0.1700.107 Safari/537.36"
        Dim httpResp As HttpWebResponse
        httpResp = DirectCast(httpReq.GetResponse(), HttpWebResponse)
        Dim respRead As New StreamReader(httpResp.GetResponseStream())
        Dim respResult As String = respRead.ReadToEnd
        If returnUrlOnly = True Then
            Dim respUrl = httpReq.GetResponse().ResponseUri.AbsoluteUri
            Return respUrl
        End If
        Return respResult
    End Function

    Public Function latestCMNightly()
        Dim getUrl As String = makeHTTPRequest("http://download.cyanogenmod.org/?device=tf300t&type=nightly")
        Dim startL As Integer = getUrl.IndexOf("<a href=""/get/jenkins/")
        If startL <> -1 Then
            Dim buildLoc As String = getUrl.Substring(startL + "<a href=""/get/jenkins/".Length())
            buildLoc = buildLoc.Substring(0, buildLoc.IndexOf(".zip") + 4)
            Dim latestUrl As String = "http://download.cyanogenmod.org/get/jenkins/" & buildLoc
            Return latestUrl
        Else
            Return ""
        End If
    End Function

    Public Function latestOmniNightly()
        Dim getUrl As String = makeHTTPRequest("http://dl.omnirom.org/tf300t/")
        Dim sepStr As String = "<a href=""/tf300t"
        Dim startL As Integer = getUrl.LastIndexOf(sepStr)
        If startL <> -1 Then
            Dim buildLoc As String = getUrl.Substring(startL + sepStr.Length())
            buildLoc = buildLoc.Substring(0, buildLoc.IndexOf(".md5sum"">"))
            Dim latestUrl As String = "http://dl.omnirom.org/tf300t" & buildLoc
            Return latestUrl
        Else
            Return ""
        End If
    End Function

    Public Function latestSuperSU()
        Dim getUrl As String = makeHTTPRequest("http://download.chainfire.eu/supersu", True)
        Dim latestUrl As String = getUrl & "?retrieve_file=1"
        Return latestUrl
    End Function

    Public Function getFileName(ByVal url As String)
        url = url.Replace("?retrieve_file=1", "")
        url = url.Substring(url.LastIndexOf("/") + 1)
        Return url.Trim()
    End Function
End Class

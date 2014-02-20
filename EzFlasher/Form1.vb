Imports RegawMOD.Android
Imports System.Threading, System.IO, System.Net

'@author Weidi Zhang (http://github.com/ebildude123)
'@license GPL v3 (https://www.gnu.org/copyleft/gpl.html)
'Copyright (C) 2014

Public Class Form1
    Dim devCheck As Thread
    Dim andControl As AndroidController
    Dim andDevice As Device
    Dim andControlFB As Fastboot
    Dim devSerial As String
    Dim chkFastboot As Boolean = False
    Dim dlClass As GetDownloads = New GetDownloads()
    Dim vendorID As String = "0x0B05"
    Dim resourceDir As String = System.AppDomain.CurrentDomain.BaseDirectory & "recovery\"
    Dim downloadDir As String = System.AppDomain.CurrentDomain.BaseDirectory & "downloads\"
    Dim WithEvents dlClient As New WebClient
    Dim dlCompleted As Boolean = False
    Dim dlFile As String
    Dim dlFileName As String
    Dim dispBytes As Boolean = False

    Public Sub writeToLog(ByVal logStr As String)
        TextBox1.AppendText("[" & DateTime.Now & "] " & logStr & Environment.NewLine)
        TextBox1.SelectionStart = TextBox1.Text.Length
        TextBox1.ScrollToCaret()
    End Sub

    Private Sub buttonEnabled(ByVal bState As Boolean)
        Button1.Enabled = bState
        Button2.Enabled = bState
        Button3.Enabled = bState
        Button4.Enabled = bState
        Button5.Enabled = bState
        Button6.Enabled = bState
        Button7.Enabled = bState
    End Sub

    Private Sub deviceCheck()
        Dim foundDev As Boolean = False
        Dim dispErr As Boolean = False

        While foundDev = False
            andControl.UpdateDeviceList()
            If andControl.HasConnectedDevices = True Then
                If andControl.ConnectedDevices.Count() > 1 Then
                    writeToLog("Multiple devices detected. Please connect only the TF300T.")
                Else
                    writeToLog("Found your device.")
                    andDevice = andControl.GetConnectedDevice(andControl.ConnectedDevices(0))
                    devSerial = andDevice.SerialNumber
                    Label6.Text = devSerial

                    Dim devMode As String = "Unknown"
                    If andDevice.State = DeviceState.RECOVERY Then
                        devMode = "ADB (Recovery)"
                    ElseIf andDevice.State = DeviceState.FASTBOOT Then
                        devMode = "Fastboot (Bootloader)"
                    ElseIf andDevice.State = DeviceState.ONLINE Then
                        devMode = "ADB (Android)"
                    End If
                    writeToLog("Device mode: " & devMode)

                    If chkFastboot = False Then
                        If andDevice.HasRoot = False Then
                            If devMode = "ADB (Recovery)" Then
                                buttonEnabled(True)
                                foundDev = True
                                Exit While
                            End If
                            If dispErr = False Then
                                writeToLog("Error: Your device is not rooted, or you have not accepted the usb debugging prompt.")
                                writeToLog("Please accept the USB debugging prompt on your device and check the always allow box.")
                                writeToLog("Waiting for device...")
                                dispErr = True
                            End If
                        Else
                            buttonEnabled(True)
                            foundDev = True
                        End If

                        Dim devModel As String = andDevice.BuildProp.GetProp("ro.product.device")
                        Label2.Text = devModel
                        Label4.Text = andDevice.BuildProp.GetProp("ro.build.version.release")

                        If devModel.ToLower() <> "tf300t" And devModel.ToLower() <> "tf300" Then
                            writeToLog("Sorry, your """ & devModel & """ is not supported. This tool is for the tf300t.")
                            Exit While
                        End If
                    Else
                        chkFastboot = False

                        buttonEnabled(True)
                        foundDev = True
                    End If
                End If
            End If

            Thread.Sleep(3000)
        End While
    End Sub

    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        andControl = AndroidController.Instance
        writeToLog("Please connect your TF300T to your computer now with android booted up.")
        writeToLog("Waiting for device in android...")
        Control.CheckForIllegalCrossThreadCalls = False

        ComboBox1.SelectedIndex = 0

        devCheck = New Thread(AddressOf deviceCheck)
        devCheck.Start()

        If Not Directory.Exists(downloadDir) Then
            Directory.CreateDirectory(downloadDir)
        End If
    End Sub

    Private Sub Form1_FormClosed(ByVal sender As System.Object, ByVal e As System.Windows.Forms.FormClosedEventArgs) Handles MyBase.FormClosed
        andControl.Dispose()
        Try
            devCheck.Abort()
        Catch ex As Exception
        End Try
    End Sub

    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click
        If (andDevice.State = DeviceState.ONLINE) Or (andDevice.State = DeviceState.RECOVERY) Then
            andDevice.RebootBootloader()

            buttonEnabled(False)
            writeToLog("Waiting for device in fastboot mode...")
            chkFastboot = True
            devCheck = New Thread(AddressOf deviceCheck)
            devCheck.Start()
        Else
            buttonEnabled(False)
            writeToLog("Waiting for device in android or recovery...")
            chkFastboot = False
            devCheck = New Thread(AddressOf deviceCheck)
            devCheck.Start()
        End If
    End Sub

    Private Sub Button3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button3.Click
        If andDevice.State = DeviceState.FASTBOOT Then
            Dim fbCmdStr As String = "-i " & vendorID & " reboot"
            Dim fbCmd As FastbootCommand = Fastboot.FormFastbootCommand(fbCmdStr)
            Dim fbResponse As String = Fastboot.ExecuteFastbootCommand(fbCmd)
            writeToLog("")
            writeToLog(fbResponse)
            writeToLog("")

            buttonEnabled(False)
            writeToLog("Waiting for device in android...")
            chkFastboot = False
            devCheck = New Thread(AddressOf deviceCheck)
            devCheck.Start()
        Else
            buttonEnabled(False)
            writeToLog("Waiting for device in fastboot mode...")
            chkFastboot = True
            devCheck = New Thread(AddressOf deviceCheck)
            devCheck.Start()
        End If
    End Sub

    Private Sub Button7_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button7.Click
        If andDevice.State = DeviceState.ONLINE Then
            andDevice.RebootRecovery()

            buttonEnabled(False)
            writeToLog("Waiting for device in recovery...")
            chkFastboot = False
            devCheck = New Thread(AddressOf deviceCheck)
            devCheck.Start()
        Else
            buttonEnabled(False)
            writeToLog("Waiting for device in android...")
            chkFastboot = False
            devCheck = New Thread(AddressOf deviceCheck)
            devCheck.Start()
        End If
    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        If andDevice.State = DeviceState.FASTBOOT Then
            Dim fbCmdStr As String = "-i " & vendorID & " flash recovery "
            Dim recovFile As String = resourceDir
            Select Case ComboBox1.SelectedIndex
                Case 0 'TWRP for KK
                    recovFile &= "twrp_kk.img"
                Case 1 'Philz for KK
                    recovFile &= "philz_kk.img"
                Case 2 'CWM for KK
                    recovFile &= "cwm_kk.img"
                Case 3 'TWRP for JB
                    recovFile &= "twrp_jb.blob"
            End Select

            If File.Exists(recovFile) Then
                fbCmdStr &= """" & recovFile & """"
                Dim fbCmd As FastbootCommand = Fastboot.FormFastbootCommand(fbCmdStr)
                Dim fbResponse As String = Fastboot.ExecuteFastbootCommand(fbCmd)
                writeToLog("")
                writeToLog(fbResponse)
                writeToLog("")
                writeToLog("A reboot is required to complete installation, rebooting...")
                fbCmdStr = "-i " & vendorID & " reboot"
                fbCmd = Fastboot.FormFastbootCommand(fbCmdStr)
                fbResponse = Fastboot.ExecuteFastbootCommand(fbCmd)
                writeToLog("")
                writeToLog(fbResponse)
                writeToLog("")
                writeToLog("Recovery successfully installed.")

                buttonEnabled(False)
                writeToLog("Waiting for device in android...")
                chkFastboot = False
                devCheck = New Thread(AddressOf deviceCheck)
                devCheck.Start()
            Else
                writeToLog("Error: Recovery resource not found. Did you tamper with the files?")
            End If
        Else
            buttonEnabled(False)
            writeToLog("Waiting for device in fastboot mode...")
            chkFastboot = True
            devCheck = New Thread(AddressOf deviceCheck)
            devCheck.Start()
        End If
    End Sub

    Private Sub dlClient_DownloadProgressChanged(ByVal sender As Object, ByVal e As DownloadProgressChangedEventArgs) Handles dlClient.DownloadProgressChanged
        Dim downloadP As Integer = e.ProgressPercentage
        ProgressBar1.Value = downloadP
        Label7.Text = downloadP.ToString() & "%"
        If dispBytes = False Then
            Dim fileSize As String = roundObjectSize(e.TotalBytesToReceive)
            writeToLog("File size: " & fileSize)
            dispBytes = True
        End If
        If downloadP = 100 And dlCompleted = False Then
            downloadCompleted()
            dlCompleted = True
        End If
    End Sub

    Private Sub downloadCompleted()
        writeToLog("Download completed.")
        If (andDevice.State = DeviceState.ONLINE) Or (andDevice.State = DeviceState.RECOVERY) Then
            Dim adbCmd As AdbCommand = Adb.FormAdbCommand("mkdir", "/sdcard/ezflasher")
            Adb.ExecuteAdbCommand(adbCmd)
            Dim pushDest As String = "/sdcard/ezflasher/" & dlFileName
            writeToLog("Pushing to location: " & pushDest)
            andDevice.PushFile(dlFile, pushDest)
            writeToLog("Push successful.")
        Else
            writeToLog("Error: Cannot push, device is not connected.")
            buttonEnabled(False)
            writeToLog("Waiting for device in android or recovery...")
            chkFastboot = False
            devCheck = New Thread(AddressOf deviceCheck)
            devCheck.Start()
        End If
    End Sub

    Private Sub Button4_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button4.Click
        If (andDevice.State = DeviceState.ONLINE) Or (andDevice.State = DeviceState.RECOVERY) Then
            dlCompleted = False
            dispBytes = False
            Dim latestDL As String = dlClass.latestSuperSU()
            Dim saveFile As String = downloadDir & dlClass.getFileName(latestDL)
            dlFile = saveFile
            dlFileName = dlClass.getFileName(latestDL)
            writeToLog("Downloading: " & latestDL)
            writeToLog("Saving To: " & saveFile)
            writeToLog("Look at the progress bar for the current status.")

            ProgressBar1.Value = ProgressBar1.Minimum
            Label7.Text = "0%"

            dlClient = New WebClient()
            dlClient.DownloadFileAsync(New Uri(latestDL), saveFile)
        Else
            buttonEnabled(False)
            writeToLog("Waiting for device in android or recovery...")
            chkFastboot = False
            devCheck = New Thread(AddressOf deviceCheck)
            devCheck.Start()
        End If
    End Sub

    Private Sub Button5_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button5.Click
        If (andDevice.State = DeviceState.ONLINE) Or (andDevice.State = DeviceState.RECOVERY) Then
            dlCompleted = False
            dispBytes = False
            Dim latestDL As String = dlClass.latestCMNightly()
            Dim saveFile As String = downloadDir & dlClass.getFileName(latestDL)
            dlFile = saveFile
            dlFileName = dlClass.getFileName(latestDL)
            writeToLog("Downloading: " & latestDL)
            writeToLog("Saving To: " & saveFile)
            writeToLog("Look at the progress bar for the current status.")

            ProgressBar1.Value = ProgressBar1.Minimum
            Label7.Text = "0%"

            dlClient = New WebClient()
            dlClient.DownloadFileAsync(New Uri(latestDL), saveFile)
        Else
            buttonEnabled(False)
            writeToLog("Waiting for device in android or recovery...")
            chkFastboot = False
            devCheck = New Thread(AddressOf deviceCheck)
            devCheck.Start()
        End If
    End Sub

    Private Sub Button6_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button6.Click
        If (andDevice.State = DeviceState.ONLINE) Or (andDevice.State = DeviceState.RECOVERY) Then
            dlCompleted = False
            dispBytes = False
            Dim latestDL As String = dlClass.latestOmniNightly()
            Dim saveFile As String = downloadDir & dlClass.getFileName(latestDL)
            dlFile = saveFile
            dlFileName = dlClass.getFileName(latestDL)
            writeToLog("Downloading: " & latestDL)
            writeToLog("Saving To: " & saveFile)
            writeToLog("Look at the progress bar for the current status.")

            ProgressBar1.Value = ProgressBar1.Minimum
            Label7.Text = "0%"

            dlClient = New WebClient()
            dlClient.DownloadFileAsync(New Uri(latestDL), saveFile)
        Else
            buttonEnabled(False)
            writeToLog("Waiting for device in android or recovery...")
            chkFastboot = False
            devCheck = New Thread(AddressOf deviceCheck)
            devCheck.Start()
        End If
    End Sub

    'The following two functions below are from CodeProject
    Private Function checkIfValueIsDecimal(ByVal value As String) As String
        Dim result As String
        If value.Contains(",") Then : result = CDbl(value).ToString("###.##")
        Else : result = CDbl(value).ToString("###.00") : End If
        Return result
    End Function

    Private Function roundObjectSize(ByVal ObjectSize As String) As String
        Dim oneByte As Integer = 1
        Dim kiloByte As Integer = 1024
        Dim megaByte As Integer = 1048576
        Dim gigaByte As Integer = 1073741824
        Dim terraByte As Long = 1099511627776
        Dim pettaByte As Long = 1125899906842624

        Select Case CLng(ObjectSize)
            Case 0 To kiloByte - 1
                If (CDbl(checkIfValueIsDecimal(CStr(CDec(ObjectSize) / oneByte))) >= 1000) = False Then
                    ObjectSize = CStr(CInt(ObjectSize) / 1) + " Bytes"
                Else : ObjectSize = "1.00 KB" : End If

            Case kiloByte To megaByte - 1
                If (CDbl(checkIfValueIsDecimal(CStr(CDec(ObjectSize) / kiloByte))) >= 1000) = False Then
                    ObjectSize = checkIfValueIsDecimal(CStr(CDec(ObjectSize) / kiloByte)) + " KB"
                Else : ObjectSize = "1.00 MB" : End If

            Case megaByte To gigaByte - 1
                If (CDbl(checkIfValueIsDecimal(CStr(CDec(ObjectSize) / megaByte))) >= 1000) = False Then
                    ObjectSize = checkIfValueIsDecimal(CStr(CDec(ObjectSize) / megaByte)) + " MB"
                Else : ObjectSize = "1.00 GB" : End If

            Case gigaByte To terraByte - 1
                If (CDbl(checkIfValueIsDecimal(CStr(CDec(ObjectSize) / gigaByte))) >= 1000) = False Then
                    ObjectSize = checkIfValueIsDecimal(CStr(CDec(ObjectSize) / gigaByte)) + " GB"
                Else : ObjectSize = "1.00 TB" : End If

            Case terraByte To pettaByte - 1
                If (CDbl(checkIfValueIsDecimal(CStr(CDec(ObjectSize) / terraByte))) >= 1000) = False Then
                    ObjectSize = checkIfValueIsDecimal(CStr(CDec(ObjectSize) / terraByte)) + " TB"
                Else : ObjectSize = "1.00 PB" : End If
        End Select
        Return ObjectSize
    End Function

    Private Sub Button8_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button8.Click
        MsgBox("To find the bootloader version, reboot into bootloader mode. Then, in the top left hand corner, you will find bootloader version.", MsgBoxStyle.Information)
    End Sub
End Class

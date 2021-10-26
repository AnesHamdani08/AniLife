Imports System.ComponentModel
Imports System.Timers
Imports Un4seen.Bass
Imports Un4seen.Bass.Misc
Public Class Player
    Public Const Version As String = "AUDX 3.0.0.0 LITE AniLife Build 1"
#Region "Events"
    Public Event PlayerStateChanged(State As State)
    Public Event VolumeChanged(NewVal As Single, IsMuted As Boolean)
    Public Event MediaLoaded(Title As String, Artist As String, Cover As System.Windows.Interop.InteropBitmap, Thumb As System.Windows.Interop.InteropBitmap, LyricsAvailable As Boolean, Lyrics As String)
    Public Event MediaEnded()
    Private CurrentMediaEndedCALLBACK As SYNCPROC
    Public Event OnFxChanged(FX As LinkHandles, State As Boolean)
    Public Event OnMediaError(ErrorCode As BASSError)
    Public Event OnRepeatChanged(NewType As RepeateBehaviour)
    Public Event OnShuffleChanged(NewType As RepeateBehaviour)
    Public Event OnEqChanged(EQgains As Integer())
    Public Event OnABLoopChanged(Item As ABLoopItem)
    Public Event EngineError(Engine As Engines, ErrorCode As BASSError)
    Public Event OnPositionChanged(newpos As Double)
#End Region
#Region "Properties"
    Private IsFirstStream As Boolean = True
    Private Owner As Window = Nothing
    Public Property PlayerState As State
    Public Property SourceURL As String
    Public Property AutoPlay As Boolean = True
    Public Property FadeAudio As Boolean = False
    Private Property Visualizer As Visuals
    Public Property Volume As Single = 0.6
    Private Property IsMute As Boolean = False
    Public Property IsInitialized As Boolean = False
    Public Property StartUpErrors As New List(Of Engines)
    Public Property IsInitializedReason As Exception = Nothing
    Public Property Mute As Boolean
        Get
            Return IsMute
        End Get
        Set(value As Boolean)
            If value Then
                Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, 0)
                IsMute = True
                RaiseEvent VolumeChanged(Volume, True)
            Else
                Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, Volume)
                IsMute = False
                RaiseEvent VolumeChanged(Volume, False)
            End If
        End Set
    End Property
    Private _RepeateType As RepeateBehaviour = RepeateBehaviour.NoRepeat
    Public Property RepeateType As RepeateBehaviour
        Get
            Return _RepeateType
        End Get
        Set(value As RepeateBehaviour)
            If value = RepeateBehaviour.RepeatOne Then
                Bass.BASS_ChannelFlags(Stream, BASSFlag.BASS_SAMPLE_LOOP, BASSFlag.BASS_SAMPLE_LOOP)
            Else
                Bass.BASS_ChannelFlags(Stream, BASSFlag.BASS_DEFAULT, BASSFlag.BASS_SAMPLE_LOOP)
            End If
            _RepeateType = value
            RaiseEvent OnRepeatChanged(value)
        End Set
    End Property
    Private _ShuffleType As RepeateBehaviour = RepeateBehaviour.NoShuffle
    Public Property Shuffle As RepeateBehaviour
        Get
            Return _ShuffleType
        End Get
        Set(value As RepeateBehaviour)
            _ShuffleType = value
            RaiseEvent OnShuffleChanged(value)
        End Set
    End Property
    Private _Mono As Boolean = False
    Public Property Mono As Boolean
        Get
            Return _Mono
        End Get
        Set(value As Boolean)
            If value = True Then
                If CurrentMediaType = StreamTypes.Local Then
                    Dim pos = GetPosition()
                    StreamStop()
                    Stream = Bass.BASS_StreamCreateFile(SourceURL, 0, 0, BASSFlag.BASS_SAMPLE_MONO Or BASSFlag.BASS_STREAM_AUTOFREE)
                    SetFx()
                    SetPosition(pos)
                    StreamPlay()
                    pos = Nothing
                    _Mono = value
                End If
            Else
                If CurrentMediaType = StreamTypes.Local Then
                    Dim pos = GetPosition()
                    StreamStop()
                    Stream = Bass.BASS_StreamCreateFile(SourceURL, 0, 0, BASSFlag.BASS_SAMPLE_FLOAT Or BASSFlag.BASS_STREAM_AUTOFREE)
                    SetFx()
                    SetPosition(pos)
                    StreamPlay()
                    pos = Nothing
                    _Mono = value
                End If
            End If
        End Set
    End Property
    Private Property _SkipSilencesInterval As Double = 500
    Private WithEvents _SkipSilencesTimer As New Timer With {.Interval = SkipSilencesInterval, .Enabled = False}
    Public Property SkipSilencesInterval As Double
        Get
            Return _SkipSilencesInterval
        End Get
        Set(value As Double)
            _SkipSilencesInterval = value
            _SkipSilencesTimer.Interval = value
        End Set
    End Property
    Private _SkipSilences As Boolean = False
    Public Property SkipSilences As Boolean
        Get
            Return _SkipSilences
        End Get
        Set(value As Boolean)
            _SkipSilences = value
            If value Then
                _SkipSilencesCuePoints = DetectSilence(SourceURL)
                _SkipSilencesTimer.Start()
            Else
                _SkipSilencesCuePoints = {0, 0}
                _SkipSilencesTimer.Stop()
            End If
        End Set
    End Property
    Private _SkipSilencesCuePoints As Double() = {0, 0}
    Private _ABLoop As ABLoopItem
    Private WithEvents _ABLoopTimer As New Timer With {.Interval = 100}
    Public WriteOnly Property ABLoop As ABLoopItem
        Set(value As ABLoopItem)
            _ABLoop = value
            SetABLoop()
        End Set
    End Property
    Private _DoubleOutput As Boolean = False
    Public Property DOStream As Integer = 0
    Private DODeviceIndex As Integer = 0
    Public Property DoubleOutput As Boolean
        Get
            Return _DoubleOutput
        End Get
        Set(value As Boolean)
            _DoubleOutput = value
            If value Then
                If CurrentMediaType <> StreamTypes.URL Then
                    If _RepeateType <> RepeateBehaviour.RepeatOne Then
                        If Mono Then
                            DOStream = Bass.BASS_StreamCreateFile(SourceURL, 0, 0, BASSFlag.BASS_SAMPLE_MONO Or BASSFlag.BASS_STREAM_AUTOFREE)
                        Else
                            DOStream = Bass.BASS_StreamCreateFile(SourceURL, 0, 0, BASSFlag.BASS_SAMPLE_FLOAT Or BASSFlag.BASS_STREAM_AUTOFREE)
                        End If
                    Else
                        If Mono Then
                            DOStream = Bass.BASS_StreamCreateFile(SourceURL, 0, 0, BASSFlag.BASS_SAMPLE_MONO Or BASSFlag.BASS_STREAM_AUTOFREE Or BASSFlag.BASS_SAMPLE_LOOP)
                        Else
                            DOStream = Bass.BASS_StreamCreateFile(SourceURL, 0, 0, BASSFlag.BASS_SAMPLE_FLOAT Or BASSFlag.BASS_STREAM_AUTOFREE Or BASSFlag.BASS_SAMPLE_LOOP)
                        End If
                    End If
                    If DOStream <> 0 Then
                        Bass.BASS_ChannelSetDevice(DOStream, DODeviceIndex)
                        Bass.BASS_ChannelSetPosition(DOStream, GetPosition)
                        If PlayerState = State.Playing Then Bass.BASS_ChannelPlay(DOStream, False)
                    End If
                End If
            Else
                If Bass.BASS_StreamFree(DOStream) Then
                    DOStream = 0
                End If
            End If
        End Set
    End Property
    Public Property CurrentProfile As PlayerProfile = Nothing

#Region "Handles"
    Public Property Stream As Integer = 0
#End Region
#Region "FX"
    Private Property _fxEQ As Integer() = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0}
    Private Property _fxEQgains As Integer() = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0}
    Public ReadOnly Property FXEQ As Integer()
        Get
            Return _fxEQ
        End Get
    End Property
    Public ReadOnly Property FXEQGains As Integer()
        Get
            Return _fxEQgains
        End Get
    End Property
    Public Property IsReverb As Boolean = False
    Public Property Reverb As BASS_DX8_REVERB = Nothing
    Private Property ReverbHandle As Integer
    Public Property IsLoudness As Boolean = False
    Private Property LoudnessHandle As Integer
    Private Property IsBalance As Boolean = False
    Private Property Balance As Single
    Private Property IsSampleRate As Boolean = False
    Private Property SampleRate As Single
    Private Property StereoMixHandle As Integer
    Public Property IsStereoMix As Boolean = False
    Private Property RotateHandle As Integer
    Public Property IsRotate As Boolean
#End Region
#Region "CurrentMedia"
    Public Property CurrentMediaType As StreamTypes
#End Region
#End Region
#Region "Boot"
    Public Sub New(WndOwner As Window, OnMediaEndedCALLBACK As SYNCPROC)
        Try
            If Not Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_DEFAULT, System.IntPtr.Zero) Then
                If Bass.BASS_ErrorGetCode <> BASSError.BASS_ERROR_ALREADY Then
                    IsInitialized = False
                    RaiseEvent EngineError(Engines.BASS, Bass.BASS_ErrorGetCode)
                    StartUpErrors.Add(Engines.BASS)
                End If
            Else
                IsInitialized = True
                Owner = WndOwner
                CurrentMediaEndedCALLBACK = OnMediaEndedCALLBACK
                Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_DEV_DEFAULT, 1)
                Visualizer = New Visuals
                PlayerState = State.Undefined
            End If
        Catch ex As Exception
            IsInitialized = False
            IsInitializedReason = ex
        End Try
    End Sub
    Public Sub Dispose()
        Bass.BASS_Free()
    End Sub
    Public Sub Init()
        Try
            If Not Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_DEFAULT, System.IntPtr.Zero) Then
                If Bass.BASS_ErrorGetCode <> BASSError.BASS_ERROR_ALREADY Then
                    IsInitialized = False
                    RaiseEvent EngineError(Engines.BASS, Bass.BASS_ErrorGetCode)
                    StartUpErrors.Add(Engines.BASS)
                End If
            Else
                IsInitialized = True
                Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_DEV_DEFAULT, 1)
                Visualizer = New Visuals
                PlayerState = State.Undefined
            End If
        Catch ex As Exception
            IsInitialized = False
            IsInitializedReason = ex
        End Try
    End Sub
#End Region
#Region "Navigation"
    Private oldvol As Single = Volume
    Public Async Sub LoadSong(Loc As String, Optional RaiseEvents As Boolean = True, Optional UseURL As Boolean = False, Optional URL As String = Nothing, Optional OverrideCurrentMedia As Boolean = False, Optional OCMTitle As String = Nothing, Optional OCMArtist As String = Nothing, Optional OCMCover As System.Drawing.Bitmap = Nothing, Optional OCMYear As Integer = 0, Optional YTURL As String = Nothing, <Runtime.CompilerServices.CallerMemberName> ByVal Optional propertyName As String = Nothing, <Runtime.CompilerServices.CallerLineNumber> ByVal Optional propertyline As String = Nothing)
        If FadeAudio AndAlso GetPosition() <> GetLength() Then
            Await FadeVol(0)
        End If
        StreamStop()
        Bass.BASS_StreamFree(Stream)
        If DoubleOutput Then
            Bass.BASS_ChannelStop(DOStream)
            Bass.BASS_StreamFree(DOStream)
        End If
        PlayerState = State.Stopped
        If RaiseEvents = True Then
            RaiseEvent PlayerStateChanged(State.Stopped)
        End If
        If Not UseURL Then
            If _RepeateType <> RepeateBehaviour.RepeatOne Then
                If Mono Then
                    Stream = Bass.BASS_StreamCreateFile(Loc, 0, 0, BASSFlag.BASS_SAMPLE_MONO Or BASSFlag.BASS_STREAM_AUTOFREE)
                Else
                    Stream = Bass.BASS_StreamCreateFile(Loc, 0, 0, BASSFlag.BASS_SAMPLE_FLOAT Or BASSFlag.BASS_STREAM_AUTOFREE)
                End If
            Else
                If Mono Then
                    Stream = Bass.BASS_StreamCreateFile(Loc, 0, 0, BASSFlag.BASS_SAMPLE_MONO Or BASSFlag.BASS_STREAM_AUTOFREE Or BASSFlag.BASS_SAMPLE_LOOP)
                Else
                    Stream = Bass.BASS_StreamCreateFile(Loc, 0, 0, BASSFlag.BASS_SAMPLE_FLOAT Or BASSFlag.BASS_STREAM_AUTOFREE Or BASSFlag.BASS_SAMPLE_LOOP)
                End If
            End If
            If _RepeateType <> RepeateBehaviour.RepeatOne Then
                If Mono Then
                    DOStream = Bass.BASS_StreamCreateFile(Loc, 0, 0, BASSFlag.BASS_SAMPLE_MONO Or BASSFlag.BASS_STREAM_AUTOFREE)
                Else
                    DOStream = Bass.BASS_StreamCreateFile(Loc, 0, 0, BASSFlag.BASS_SAMPLE_FLOAT Or BASSFlag.BASS_STREAM_AUTOFREE)
                End If
            Else
                If Mono Then
                    DOStream = Bass.BASS_StreamCreateFile(Loc, 0, 0, BASSFlag.BASS_SAMPLE_MONO Or BASSFlag.BASS_STREAM_AUTOFREE Or BASSFlag.BASS_SAMPLE_LOOP)
                Else
                    DOStream = Bass.BASS_StreamCreateFile(Loc, 0, 0, BASSFlag.BASS_SAMPLE_FLOAT Or BASSFlag.BASS_STREAM_AUTOFREE Or BASSFlag.BASS_SAMPLE_LOOP)
                End If
            End If
            If DOStream <> 0 Then
                Bass.BASS_ChannelSetDevice(DOStream, DODeviceIndex)
                Bass.BASS_ChannelSetPosition(DOStream, GetPosition)
                If PlayerState = State.Playing Then Bass.BASS_ChannelPlay(DOStream, False)
            End If
            If Stream <> 0 Then
                Bass.BASS_ChannelSetSync(Stream, BASSSync.BASS_SYNC_END Or BASSSync.BASS_SYNC_MIXTIME, 0, CurrentMediaEndedCALLBACK, IntPtr.Zero)
                If FadeAudio Then
                    SetVolume(Volume, False, True)
                Else
                    SetVolume(Volume, True, True)
                End If
                SourceURL = Loc
                SetFx()
                CurrentMediaType = StreamTypes.Local
                PlayerState = State.MediaLoaded
                If RaiseEvents = True Then
                    RaiseEvent MediaLoaded(Nothing, Nothing, Nothing, Nothing, False, Nothing)
                    RaiseEvent PlayerStateChanged(State.MediaLoaded)
                End If
                If SkipSilences Then
                    _SkipSilencesCuePoints = DetectSilence(SourceURL)
                    SetPosition(_SkipSilencesCuePoints(0))
                    _SkipSilencesTimer.Start()
                End If
            Else
                RaiseEvent OnMediaError(Bass.BASS_ErrorGetCode)
            End If
        Else
            If URL IsNot Nothing Then
                If _RepeateType <> RepeateBehaviour.RepeatOne Then
                    Stream = Bass.BASS_StreamCreateURL(URL, 0, BASSFlag.BASS_SAMPLE_FLOAT Or BASSFlag.BASS_STREAM_AUTOFREE Or BASSFlag.BASS_STREAM_STATUS, Nothing, Nothing)
                Else
                    Stream = Bass.BASS_StreamCreateURL(URL, 0, BASSFlag.BASS_SAMPLE_FLOAT Or BASSFlag.BASS_STREAM_AUTOFREE Or BASSFlag.BASS_STREAM_STATUS Or BASSFlag.BASS_SAMPLE_LOOP, Nothing, Nothing)
                End If
                If Stream <> 0 Then
                    Bass.BASS_ChannelSetSync(Stream, BASSSync.BASS_SYNC_END, 0, CurrentMediaEndedCALLBACK, IntPtr.Zero)
                    SetVolume(Volume, True, True)
                    SetFx()
                    If OverrideCurrentMedia = False Then
                        SourceURL = URL
                        CurrentMediaType = StreamTypes.URL
                    Else
                        If Not String.IsNullOrEmpty(YTURL) Then
                            SourceURL = YTURL
                            CurrentMediaType = StreamTypes.Youtube
                        Else
                            SourceURL = URL
                            CurrentMediaType = StreamTypes.URL
                        End If
                    End If
                    PlayerState = State.MediaLoaded
                    If RaiseEvents = True Then
                        RaiseEvent MediaLoaded(Nothing, Nothing, Nothing, Nothing, False, Nothing)
                        RaiseEvent PlayerStateChanged(State.MediaLoaded)
                    End If
                Else
                    RaiseEvent OnMediaError(Bass.BASS_ErrorGetCode)
                End If
            Else
                Exit Sub
            End If
        End If
        If AutoPlay Then
            StreamPlay()
        End If
    End Sub
    Public Async Sub StreamPlay(Optional RaiseEvents As Boolean = True)
        If Bass.BASS_ChannelPlay(Stream, False) Then
            If RaiseEvents Then
                RaiseEvent PlayerStateChanged(State.Playing)
                PlayerState = State.Playing
            End If
            If DoubleOutput Then
                Bass.BASS_ChannelPlay(DOStream, False)
            End If
            If FadeAudio Then
                Await FadeVol(oldvol)
            End If
        End If
    End Sub
    Public Async Sub StreamPause(Optional RaiseEvents As Boolean = True)
        If FadeAudio Then
            Await FadeVol(0)
        End If
        If Bass.BASS_ChannelPause(Stream) Then
            If RaiseEvents Then
                RaiseEvent PlayerStateChanged(State.Paused)
                PlayerState = State.Paused
            End If
            Bass.BASS_ChannelPause(DOStream)
        Else
            SetVolume(oldvol, True, True)
        End If
    End Sub
    Public Sub StreamToggle(Optional RaiseEvents As Boolean = True)
        If PlayerState = State.Playing Then
            StreamPause(RaiseEvents)
        Else
            StreamPlay(RaiseEvents)
        End If
    End Sub
    Public Sub StreamStop()
        If Bass.BASS_ChannelStop(Stream) Then
            RaiseEvent PlayerStateChanged(State.Stopped)
            PlayerState = State.Stopped
            Stream = 0
            If DoubleOutput Then
                Bass.BASS_ChannelStop(DOStream)
                DOStream = 0
            End If
        End If
    End Sub
#End Region
#Region "Settings"
    Public Sub SetVolume(Vol As Single, Optional RaiseEvents As Boolean = True, Optional IsFadeAudioCalled As Boolean = False)
        Select Case Vol
            Case < 0
                If Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, 0) Then
                    Volume = 0
                    If RaiseEvents = True Then
                        RaiseEvent VolumeChanged(0, False)
                    End If
                End If
            Case > 1
                If Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, 1) Then
                    Volume = 1
                    If RaiseEvents = True Then
                        RaiseEvent VolumeChanged(1, False)
                    End If
                End If
            Case Else
                If Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, Vol) Then
                    Volume = Vol
                    If RaiseEvents = True Then
                        RaiseEvent VolumeChanged(Vol, False)
                    End If
                End If
        End Select
        If DoubleOutput Then
            Bass.BASS_ChannelSetAttribute(DOStream, BASSAttribute.BASS_ATTRIB_VOL, Volume)
        End If
        If IsFadeAudioCalled = False Then
            oldvol = Volume
        End If
    End Sub
    Public Function GetVolume() As Single
        Dim Vol As Single = -1
        If Bass.BASS_ChannelGetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, Vol) Then
            Volume = Vol
            Return Vol
        End If
        Return Vol
    End Function
    Public Sub SetPosition(Seconds As Double)
        If Bass.BASS_ChannelSetPosition(Stream, Seconds) Then RaiseEvent OnPositionChanged(Seconds)
        If DoubleOutput Then
            Bass.BASS_ChannelSetPosition(DOStream, Seconds)
        End If
    End Sub
    Public Function GetPosition() As Long
        Try
            Return Bass.BASS_ChannelBytes2Seconds(Stream, Bass.BASS_ChannelGetPosition(Stream, BASSMode.BASS_POS_BYTE))
        Catch ex As Exception
            Return -1
        End Try
    End Function
    Public Function GetPrecisePosition() As Double
        Try
            Return Bass.BASS_ChannelBytes2Seconds(Stream, Bass.BASS_ChannelGetPosition(Stream, BASSMode.BASS_POS_BYTE))
        Catch ex As Exception
            Return -1
        End Try
    End Function
    Public Function GetLength() As Long
        Try
            Return Bass.BASS_ChannelBytes2Seconds(Stream, Bass.BASS_ChannelGetLength(Stream, BASSMode.BASS_POS_BYTE))
        Catch ex As Exception
            Return 0
        End Try
    End Function
    Public Function GetPreciseLength() As Double
        Try
            Return Bass.BASS_ChannelBytes2Seconds(Stream, Bass.BASS_ChannelGetLength(Stream, BASSMode.BASS_POS_BYTE))
        Catch ex As Exception
            Return 0
        End Try
    End Function
    Public Sub UpdateEQ(band As Integer, gain As Single, Optional DisableForAll As Boolean = False, Optional RaiseEvents As Boolean = True)
        If DisableForAll = False Then
            Dim eq As New BASS_DX8_PARAMEQ()
            If Bass.BASS_FXGetParameters(_fxEQ(band), eq) Then
                eq.fGain = gain
                Bass.BASS_FXSetParameters(_fxEQ(band), eq)
                _fxEQgains(band) = gain
                If RaiseEvents Then RaiseEvent OnFxChanged(LinkHandles.EQ, True)
            End If
        Else
            SetEq(True)
            If RaiseEvents Then RaiseEvent OnFxChanged(LinkHandles.EQ, False)
        End If
    End Sub
    Private Sub SetEq(Optional Reset As Boolean = False)
        If Reset = False Then
            ' 10-band EQ
            If _fxEQgains(0) <> 0 Or _fxEQgains(1) <> 0 Or _fxEQgains(2) <> 0 Or _fxEQgains(3) <> 0 Or _fxEQgains(4) <> 0 Or _fxEQgains(5) <> 0 Or _fxEQgains(6) <> 0 Or _fxEQgains(7) <> 0 Or _fxEQgains(8) <> 0 Or _fxEQgains(9) <> 0 Then
                RaiseEvent OnFxChanged(LinkHandles.EQ, True)
            End If
            Dim eq As New BASS_DX8_PARAMEQ()
            _fxEQ(0) = Bass.BASS_ChannelSetFX(Stream, BASSFXType.BASS_FX_DX8_PARAMEQ, 0)
            _fxEQ(1) = Bass.BASS_ChannelSetFX(Stream, BASSFXType.BASS_FX_DX8_PARAMEQ, 0)
            _fxEQ(2) = Bass.BASS_ChannelSetFX(Stream, BASSFXType.BASS_FX_DX8_PARAMEQ, 0)
            _fxEQ(3) = Bass.BASS_ChannelSetFX(Stream, BASSFXType.BASS_FX_DX8_PARAMEQ, 0)
            _fxEQ(4) = Bass.BASS_ChannelSetFX(Stream, BASSFXType.BASS_FX_DX8_PARAMEQ, 0)
            _fxEQ(5) = Bass.BASS_ChannelSetFX(Stream, BASSFXType.BASS_FX_DX8_PARAMEQ, 0)
            _fxEQ(6) = Bass.BASS_ChannelSetFX(Stream, BASSFXType.BASS_FX_DX8_PARAMEQ, 0)
            _fxEQ(7) = Bass.BASS_ChannelSetFX(Stream, BASSFXType.BASS_FX_DX8_PARAMEQ, 0)
            _fxEQ(8) = Bass.BASS_ChannelSetFX(Stream, BASSFXType.BASS_FX_DX8_PARAMEQ, 0)
            _fxEQ(9) = Bass.BASS_ChannelSetFX(Stream, BASSFXType.BASS_FX_DX8_PARAMEQ, 0)
            eq.fBandwidth = 18.0F
            eq.fCenter = 80.0F
            eq.fGain = _fxEQgains(0)
            Bass.BASS_FXSetParameters(_fxEQ(0), eq)
            eq.fCenter = 100.0F
            eq.fGain = _fxEQgains(1)
            Bass.BASS_FXSetParameters(_fxEQ(1), eq)
            eq.fCenter = 125.0F
            eq.fGain = _fxEQgains(2)
            Bass.BASS_FXSetParameters(_fxEQ(2), eq)
            eq.fCenter = 250.0F
            eq.fGain = _fxEQgains(3)
            Bass.BASS_FXSetParameters(_fxEQ(3), eq)
            eq.fCenter = 500.0F
            eq.fGain = _fxEQgains(4)
            Bass.BASS_FXSetParameters(_fxEQ(4), eq)
            eq.fCenter = 1000.0F
            eq.fGain = _fxEQgains(5)
            Bass.BASS_FXSetParameters(_fxEQ(5), eq)
            eq.fCenter = 2000.0F
            eq.fGain = _fxEQgains(6)
            Bass.BASS_FXSetParameters(_fxEQ(6), eq)
            eq.fCenter = 4000.0F
            eq.fGain = _fxEQgains(7)
            Bass.BASS_FXSetParameters(_fxEQ(7), eq)
            eq.fCenter = 8000.0F
            eq.fGain = _fxEQgains(8)
            Bass.BASS_FXSetParameters(_fxEQ(8), eq)
            eq.fCenter = 16000.0F
            eq.fGain = _fxEQgains(9)
            Bass.BASS_FXSetParameters(_fxEQ(9), eq)
        Else
            _fxEQgains = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0}
            ' 10-band EQ
            Dim eq As New BASS_DX8_PARAMEQ()
            eq.fBandwidth = 18.0F
            eq.fCenter = 80.0F
            eq.fGain = _fxEQgains(0)
            Bass.BASS_FXSetParameters(_fxEQ(0), eq)
            eq.fCenter = 100.0F
            eq.fGain = _fxEQgains(1)
            Bass.BASS_FXSetParameters(_fxEQ(1), eq)
            eq.fCenter = 125.0F
            eq.fGain = _fxEQgains(2)
            Bass.BASS_FXSetParameters(_fxEQ(2), eq)
            eq.fCenter = 250.0F
            eq.fGain = _fxEQgains(3)
            Bass.BASS_FXSetParameters(_fxEQ(3), eq)
            eq.fCenter = 500.0F
            eq.fGain = _fxEQgains(4)
            Bass.BASS_FXSetParameters(_fxEQ(4), eq)
            eq.fCenter = 1000.0F
            eq.fGain = _fxEQgains(5)
            Bass.BASS_FXSetParameters(_fxEQ(5), eq)
            eq.fCenter = 2000.0F
            eq.fGain = _fxEQgains(6)
            Bass.BASS_FXSetParameters(_fxEQ(6), eq)
            eq.fCenter = 4000.0F
            eq.fGain = _fxEQgains(7)
            Bass.BASS_FXSetParameters(_fxEQ(7), eq)
            eq.fCenter = 8000.0F
            eq.fGain = _fxEQgains(8)
            Bass.BASS_FXSetParameters(_fxEQ(8), eq)
            eq.fCenter = 16000.0F
            eq.fGain = _fxEQgains(9)
            Bass.BASS_FXSetParameters(_fxEQ(9), eq)
        End If
    End Sub
    Public Sub UpdateReverb(InGain As Single, ReverbMix As Single, ReverbTime As Single, HighFreqRTRatio As Single, UpInGain As Boolean, UpReverbMix As Boolean, UpReverbTime As Boolean, UpHighFreqRTRatio As Boolean, Optional DisableFX As Boolean = False, Optional EnableFX As Boolean = False)
        If EnableFX = True AndAlso IsReverb = False Then
            Dim Rvrb As New BASS_DX8_REVERB
            Rvrb.Preset_Default()
            ReverbHandle = Bass.BASS_ChannelSetFX(Stream, BASSFXType.BASS_FX_DX8_REVERB, 1)
            If ReverbHandle <> 0 Then
                IsReverb = True
                Reverb = Rvrb
                RaiseEvent OnFxChanged(LinkHandles.Reverb, True)
            End If
            Exit Sub
        End If
        If DisableFX = True AndAlso IsReverb = True Then
            If Bass.BASS_ChannelRemoveFX(Stream, ReverbHandle) Then
                ReverbHandle = 0
                IsReverb = False
                RaiseEvent OnFxChanged(LinkHandles.Reverb, False)
            End If
        End If
        If IsReverb = True Then
            Dim Rvrb As New BASS_DX8_REVERB
            Bass.BASS_FXGetParameters(ReverbHandle, Rvrb)
            If UpInGain Then
                Rvrb.fInGain = InGain
            End If
            If UpReverbMix Then
                Rvrb.fReverbMix = ReverbMix
            End If
            If UpReverbTime Then
                Rvrb.fReverbTime = ReverbTime
            End If
            If UpHighFreqRTRatio Then
                Rvrb.fHighFreqRTRatio = HighFreqRTRatio
            End If
            If Bass.BASS_FXSetParameters(ReverbHandle, Rvrb) Then
                Reverb = Rvrb
            End If
        End If
    End Sub
    Private Sub SetReverb()
        If Reverb IsNot Nothing Then
            ReverbHandle = Bass.BASS_ChannelSetFX(Stream, BASSFXType.BASS_FX_DX8_REVERB, 1)
            If ReverbHandle <> 0 Then
                Bass.BASS_FXSetParameters(ReverbHandle, Reverb)
            End If
        End If
    End Sub
    Private Sub SetFx()
        SetEq() 'Equalizer
        If IsReverb Then 'Reverb
            SetReverb()
            RaiseEvent OnFxChanged(LinkHandles.Reverb, True)
        Else
            RaiseEvent OnFxChanged(LinkHandles.Reverb, False)
        End If
        If IsBalance Then 'Balance
            SetBalance(Balance)
            RaiseEvent OnFxChanged(LinkHandles.Balance, True)
        Else
            RaiseEvent OnFxChanged(LinkHandles.Balance, False)
        End If
        If IsSampleRate Then 'Sample Rate
            SetSampleRate(SampleRate)
            RaiseEvent OnFxChanged(LinkHandles.SampleRate, True)
        Else
            RaiseEvent OnFxChanged(LinkHandles.SampleRate, False)
        End If
    End Sub
    Public Sub ClearFx()
        UpdateEQ(0, 0, True) 'Equalizer
        UpdateReverb(0, 0, 0, 0, 0, 0, 0, 0, True) 'Reverb               
        SetBalance(0) 'Balance
        SetBalance(Balance)
        SetSampleRate(0) 'Sample Rate                      
    End Sub
    Public Function GetHandle(FX As LinkHandles) As String
        Select Case FX
            Case LinkHandles.Stream
                Return Stream
            Case LinkHandles.EQ
                Return String.Join(vbCrLf, _fxEQ)
            Case LinkHandles.Reverb
                Return ReverbHandle
            Case LinkHandles.Loudness
                Return LoudnessHandle
            Case LinkHandles.BASS
                Return Bass.BASS_GetVersion
        End Select
        Return "Null"
    End Function
    Public Sub SetBalance(_Balance As Single)
        Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_PAN, _Balance)
        If _Balance = 0 Then
            IsBalance = False
            Balance = 0
            RaiseEvent OnFxChanged(LinkHandles.Balance, False)
        Else
            IsBalance = True
            Balance = _Balance
            RaiseEvent OnFxChanged(LinkHandles.Balance, True)
        End If
    End Sub
    Public Function GetBalance() As Single
        Return Balance
    End Function
    Public Function GetPeak() As Utils.PeakItem
        Dim Level = Bass.BASS_ChannelGetLevel(Stream)
        Dim LeftLevel = Un4seen.Bass.Utils.LowWord32(Level) / 1000
        Dim RightLevel = Un4seen.Bass.Utils.HighWord32(Level) / 1000
        Return New Utils.PeakItem((LeftLevel + RightLevel) / 2, LeftLevel, RightLevel)
    End Function
    Public Sub SetSampleRate(Rate As Single)
        Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_FREQ, Rate)
        If Rate = 0 Then
            IsSampleRate = False
            SampleRate = 0
            RaiseEvent OnFxChanged(LinkHandles.SampleRate, False)
        Else
            IsSampleRate = True
            SampleRate = Rate
            RaiseEvent OnFxChanged(LinkHandles.SampleRate, True)
        End If
    End Sub
    Public Sub SetCustomState(state As State)
        RaiseEvent PlayerStateChanged(state)
    End Sub
    Public Function DetectSilence(Filename As String) As Double()
        Dim cueInPos As Double
        Dim cueOutPos As Double
        Dim ComInfo As Double() = {0, 0}
        If Un4seen.Bass.Utils.DetectCuePoints(Filename, 10, cueInPos, cueOutPos, -25, -42, 0) Then
            ComInfo = {cueInPos, cueOutPos}
        Else
            ComInfo = {0, 9999}
        End If
        Return ComInfo
    End Function
    Private Sub _SkipSilencesTimer_Elapsed(sender As Object, e As ElapsedEventArgs) Handles _SkipSilencesTimer.Elapsed
        If SkipSilences Then
            Dim pos = GetPosition()
            If pos <> -1 Then
                If pos >= _SkipSilencesCuePoints(1) Then
                    _SkipSilencesTimer.Stop()
                    CurrentMediaEndedCALLBACK(Stream, 0, Nothing, IntPtr.Zero)
                End If
            End If
        Else
            _SkipSilencesTimer.Stop()
        End If
    End Sub
#Region "Sound Fading"
    Dim WithEvents FaderUpTimer As New Timer With {.Interval = 1}
    Dim WithEvents FaderDownTimer As New Timer With {.Interval = 1}
    Dim WithEvents PartialFaderDownTimer As New Timer With {.Interval = 1}
    Dim PartialDownTo As Single = 0
    Dim FadingVol As Boolean = False
    Public ReadOnly Property IsFadingVol As Boolean
        Get
            Return FadingVol
        End Get
    End Property
    <Obsolete("FadeVolume Is Obsolete Use FadeVol for better performance")>
    Public Async Function FadeVolume(Up As Boolean, Down As Boolean, Optional PartialFade As Boolean = False, Optional PartialTo As Single = 0) As Task(Of Boolean)
        If PartialFade = True Then
            FadingVol = True
            PartialDownTo = PartialTo
            FaderUpTimer.Stop()
            FaderDownTimer.Stop()
            PartialFaderDownTimer.Start()
            Await Task.Delay(650)
            Return Await Task.FromResult(True)
        Else
            If Up Then
                FadingVol = True
                PartialFaderDownTimer.Stop()
                FaderDownTimer.Stop()
                FaderUpTimer.Start()
                Await Task.Delay(650)
                Return Await Task.FromResult(True)
            ElseIf Down Then
                FadingVol = True
                FaderUpTimer.Stop()
                PartialFaderDownTimer.Stop()
                FaderDownTimer.Start()
                Await Task.Delay(650)
                Return Await Task.FromResult(True)
            End If
        End If
    End Function
    Private Sub FaderUpTimer_Tick() Handles FaderUpTimer.Elapsed
        Dim vol As Single = 0F
        Bass.BASS_ChannelGetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, vol)
        If vol >= Volume Then
            FaderUpTimer.Stop()
            FadingVol = False
            Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, Volume)
        Else
            FadingVol = True
            If Not Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, vol + 0.025) Then
                Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, 1)
            End If
        End If
    End Sub
    Private Sub FaderDownTimer_Tick() Handles FaderDownTimer.Elapsed
        Dim vol As Single = 0F
        Bass.BASS_ChannelGetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, vol)
        If vol <= 0 Then
            FaderDownTimer.Stop()
            FadingVol = False
            Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, 0)
        Else
            FadingVol = True
            If Not Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, vol - 0.025) Then
                Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, 0)
            End If
        End If
    End Sub
    Private Sub PartialFaderDownTimer_Tick() Handles PartialFaderDownTimer.Elapsed
        Dim vol As Single = 0F
        Bass.BASS_ChannelGetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, vol)
        If vol <= PartialDownTo Then
            FaderDownTimer.Stop()
            FadingVol = False
            Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, PartialDownTo)
        Else
            FadingVol = True
            If Not Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, vol - 0.025) Then
                Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, 0)
            End If
        End If
    End Sub
    Private FadeVolDB As Boolean = False
    Public Async Function FadeVol(ToVol As Single, Optional Delay As Integer = 1, Optional DoubleBuffer As Boolean = True) As Task
        Await Task.Run(Sub()
                           If DoubleBuffer = False Then
                               Select Case ToVol
                                   Case < Volume 'Down
                                       Do While ToVol <> Volume AndAlso Volume >= ToVol
                                           If ToVol = Volume Then Exit Do
                                           If Delay <> -1 Then Threading.Thread.Sleep(Delay)
                                           SetVolume(Volume - 0.001, False, True)
                                       Loop
                                   Case > Volume 'Up
                                       Do While ToVol <> Volume AndAlso Volume <= ToVol
                                           If ToVol = Volume Then Exit Do
                                           If Delay <> -1 Then Threading.Thread.Sleep(Delay)
                                           SetVolume(Volume + 0.001, False, True)
                                       Loop
                                   Case = Volume 'Nothing
                               End Select
                           Else
                               Select Case ToVol
                                   Case < Volume 'Down
                                       Do While ToVol <> Volume AndAlso Volume >= ToVol
                                           If ToVol = Volume Then Exit Do
                                           If FadeVolDB = True Then
                                               If Delay <> -1 Then Threading.Thread.Sleep(Delay)
                                               FadeVolDB = Not FadeVolDB
                                           Else
                                               FadeVolDB = Not FadeVolDB
                                           End If
                                           SetVolume(Volume - 0.001, False, True)
                                       Loop
                                   Case > Volume 'Up
                                       Do While ToVol <> Volume AndAlso Volume <= ToVol
                                           If ToVol = Volume Then Exit Do
                                           If FadeVolDB = True Then
                                               If Delay <> -1 Then Threading.Thread.Sleep(Delay)
                                               FadeVolDB = Not FadeVolDB
                                           Else
                                               FadeVolDB = Not FadeVolDB
                                           End If
                                           SetVolume(Volume + 0.001, False, True)
                                       Loop
                                   Case = Volume 'Nothing
                               End Select
                           End If
                       End Sub)
    End Function
#End Region
    Public Function GetOutputDevices() As List(Of String)
        Dim n As Integer = 0
        Dim DevicesList As New List(Of String)
        Dim info As New BASS_DEVICEINFO()
        While (Bass.BASS_GetDeviceInfo(n, info))
            DevicesList.Add(info.ToString)
            n += 1
        End While
        Return DevicesList
    End Function
    Public Sub SetOutputDevice(index As Integer)
        Bass.BASS_Init(index, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero)
        Bass.BASS_SetDevice(index)
        Bass.BASS_ChannelSetDevice(Stream, index)
    End Sub
    Public Sub SetDoubleOutputDevice(index As Integer)
        Bass.BASS_Init(index, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero)
        Bass.BASS_ChannelSetDevice(DOStream, index)
        DODeviceIndex = index
    End Sub
    Private Sub SetABLoop()
        If _ABLoop IsNot Nothing Then
            _ABLoopTimer.Start()
        Else
            _ABLoopTimer.Stop()
        End If
        RaiseEvent OnABLoopChanged(_ABLoop)
    End Sub
    Private Sub _ABLoopTimer_Elapsed(sender As Object, e As ElapsedEventArgs) Handles _ABLoopTimer.Elapsed
        If GetPosition() >= _ABLoop.B Then
            SetPosition(_ABLoop.A)
        End If
    End Sub
    Public Sub LoadProfile(prof As PlayerProfile)
        ClearFx()
        For Each fx In prof.FXs
            Select Case fx.Type
                Case LinkHandles.EQ
                    CType(fx.obj, List(Of Integer)).CopyTo(_fxEQgains)
                    SetEq() 'EQ
                Case LinkHandles.Reverb
                    Reverb = fx.obj
                    IsReverb = True
                    SetReverb() 'Reverb                    
                    RaiseEvent OnFxChanged(LinkHandles.Reverb, True)
                Case LinkHandles.Balance
                    IsBalance = True
                    SetBalance(fx.obj) 'Balance
                    RaiseEvent OnFxChanged(LinkHandles.Balance, True)
                Case LinkHandles.SampleRate
                    IsSampleRate = True
                    SetSampleRate(fx.obj) 'Samplerate
                    RaiseEvent OnFxChanged(LinkHandles.SampleRate, True)
            End Select
        Next
        CurrentProfile = prof
    End Sub
    Public Function SaveProfile() As PlayerProfile
        Dim prof As New PlayerProfile With {.Name = "Custom"}
        If _fxEQgains(0) <> 0 Or _fxEQgains(1) <> 0 Or _fxEQgains(2) <> 0 Or _fxEQgains(3) <> 0 Or _fxEQgains(4) <> 0 Or _fxEQgains(5) <> 0 Or _fxEQgains(6) <> 0 Or _fxEQgains(7) <> 0 Or _fxEQgains(8) <> 0 Or _fxEQgains(9) <> 0 Then
            Dim FXEQGAINS As New List(Of Integer) 'EQ
            For Each gain In _fxEQgains
                FXEQGAINS.Add(gain)
            Next
            prof.FXs.Add(New PlayerProfile.FX(LinkHandles.EQ, FXEQGAINS))
        End If
        If IsReverb Then 'Reverb
            prof.FXs.Add(New PlayerProfile.FX(LinkHandles.Reverb, Reverb))
        End If
        If IsBalance Then 'Balance
            prof.FXs.Add(New PlayerProfile.FX(LinkHandles.Balance, Balance))
        End If
        If IsSampleRate Then 'Sample Rate
            prof.FXs.Add(New PlayerProfile.FX(LinkHandles.SampleRate, SampleRate))
        End If
        CurrentProfile = prof
        Return prof
    End Function
    Public Function GetChannelSampleRate() As Single
        Dim val As Single
        Bass.BASS_ChannelGetAttribute(Stream, BASSAttribute.BASS_ATTRIB_FREQ, val)
        Return val
    End Function
#End Region
#Region "Visuals"
    Public Function CreateVisualizer(type As Visualizers, width As Integer, height As Integer, color1 As System.Drawing.Color, color2 As System.Drawing.Color, color3 As System.Drawing.Color, background As System.Drawing.Color, linewidth As Integer, peakwidth As Integer, distance As Integer, peakdelay As Integer, linear As Boolean, fullspectrum As Boolean, highquality As Boolean) As System.Drawing.Bitmap
        If type = Visualizers.Line Then
            Return Visualizer.CreateSpectrumLine(Stream, width, height, color1, color2, background, linewidth, distance, True, False, False)
        ElseIf type = Visualizers.Wave Then
            Return Visualizer.CreateWaveForm(Stream, width, height, color1, color2, System.Drawing.Color.Empty, background, linewidth, False, True, False)
        ElseIf type = Visualizers.Spectrum Then
            Return Visualizer.CreateSpectrum(Stream, width, height, color1, color2, background, linear, fullspectrum, highquality)
        ElseIf type = Visualizers.SpectumLine Then
            Return Visualizer.CreateSpectrumLine(Stream, width, height, color1, color2, background, linewidth, distance, linear, fullspectrum, highquality)
        ElseIf type = Visualizers.SpectrumPeak Then
            Return Visualizer.CreateSpectrumLinePeak(Stream, width, height, color1, color2, color3, background, linewidth, peakwidth, distance, peakdelay, linear, fullspectrum, highquality)
        Else
            Return Nothing
        End If
    End Function
#End Region
#Region "Error Resolvers"
    Public Async Sub ReSendPlayStateChangedEvent(state As State, Wait As Integer, <Runtime.CompilerServices.CallerMemberName> ByVal Optional propertyName As String = Nothing, <Runtime.CompilerServices.CallerLineNumber> ByVal Optional propertyLine As String = Nothing)
        Try
            Await Task.Delay(Wait)
            RaiseEvent PlayerStateChanged(state)
        Catch ex As Exception
        End Try
    End Sub
#End Region
#Region "Enums"
    Public Enum State
        Undefined = 0
        MediaLoaded = 1
        Playing = 2
        Paused = 3
        Stopped = 4
        _Error = 5
    End Enum
    Public Enum Visualizers
        Line = 0
        Wave = 1
        Spectrum = 2
        SpectumLine = 3
        SpectrumPeak = 4
    End Enum
    Public Enum LinkHandles
        Stream = 0
        EQ = 1
        Reverb = 2
        Loudness = 3
        FX = 4
        SFX = 5
        BASS = 6
        Balance = 7
        SampleRate = 8
        StereoMix = 9
        Rotate = 10
    End Enum
    Public Enum StreamTypes
        URL = 0
        Youtube = 1
        Soundcloud = 2
        Local = 3
    End Enum

    Public Enum RepeateBehaviour
        NoRepeat = 0
        RepeatOne = 1
        RepeatAll = 2
        Shuffle = 3
        NoShuffle = 4
    End Enum
    Public Enum RotatePreset
        Slow
        Med
        Fast
    End Enum
    Public Enum Engines
        BASS
        BASS_FX
        BASS_SFX
        BASS_WADSP
        BASS_FLAC
        BASS_MIDI
    End Enum
#End Region
#Region "Classes"
    Public Class ABLoopItem
        Private Property _A As Double
        Private Property _B As Double
        Public ReadOnly Property A As Double
            Get
                Return _A
            End Get
        End Property
        Public ReadOnly Property B As Double
            Get
                Return _B
            End Get
        End Property
        Public Sub New(A As Double, B As Double)
            _A = A
            _B = B
        End Sub
    End Class
    Public Class PlayerProfile
        Public Class FX
            Public Property Type As LinkHandles
            Public Property obj As Object
            Public Sub New(_type As LinkHandles, par As Object)
                Type = _type
                obj = par
            End Sub
        End Class
        Public Property Name As String
        Public Property FXs As List(Of FX)
        Public Sub New()
            FXs = New List(Of FX)
        End Sub
        Public Overrides Function ToString() As String
            Dim sb As New Text.StringBuilder
            For Each fx In FXs
                Select Case fx.Type
                    Case LinkHandles.EQ
                        sb.Append(1 & ">")
                        sb.Append(String.Join(">", CType(fx.obj, List(Of Integer))))
                        sb.Append("<fx>")
                    Case LinkHandles.Reverb
                        Dim cfx = CType(fx.obj, BASS_DX8_REVERB)
                        sb.Append(2 & ">" & cfx.fHighFreqRTRatio & ">" & cfx.fInGain & ">" & cfx.fReverbMix & ">" & cfx.fReverbTime & "<fx>")
                    Case LinkHandles.Balance
                        sb.Append(7 & ">" & fx.obj & "<fx>")
                    Case LinkHandles.SampleRate
                        sb.Append(8 & ">" & fx.obj & "<fx>")
                End Select
            Next
            Return sb.ToString
        End Function
        Public Shared Function FromString(str As String) As PlayerProfile
            Dim rprof As New PlayerProfile
            Dim pars = str.Split("<fx>")
            For Each fx In pars
                Dim s_fx = fx.Split(">")
                If s_fx(0) = "fx" Then Utils.RemoveAt(s_fx, 0)
                Try
                    Select Case CType(s_fx(0), LinkHandles)
                        Case LinkHandles.EQ
                            Utils.RemoveAt(s_fx, 0)
                            rprof.FXs.Add(New PlayerProfile.FX(LinkHandles.EQ, New List(Of Integer)(s_fx)))
                        Case LinkHandles.Reverb
                            Utils.RemoveAt(s_fx, 0)
                            Dim _fx As New BASS_DX8_REVERB
                            _fx.fHighFreqRTRatio = s_fx(0) : _fx.fInGain = s_fx(1) : _fx.fReverbMix = s_fx(2) : _fx.fReverbTime = s_fx(3)
                            rprof.FXs.Add(New PlayerProfile.FX(LinkHandles.Reverb, _fx))
                        Case LinkHandles.Balance
                            rprof.FXs.Add(New PlayerProfile.FX(LinkHandles.Balance, s_fx(1)))
                        Case LinkHandles.SampleRate
                            rprof.FXs.Add(New PlayerProfile.FX(LinkHandles.SampleRate, s_fx(1)))
                    End Select
                Catch
                End Try
            Next
            Return rprof
        End Function
    End Class
#End Region
End Class
''' <summary>
''' Provides methods to get or set built-in resources
''' </summary>
Public Class AniResolver
    Public Const LinkVersion As String = "0.0.0.1-Beta.4"
#Region "Localization"
    Public Shared Property APPNAME As String
        Get
            Return Application.Current.Resources("L_APPNAME")
        End Get
        Set(value As String)
            Application.Current.Resources("L_APPNAME") = value
        End Set
    End Property
    Public Shared Property HOME As String
        Get
            Return Application.Current.Resources("L_HOME")
        End Get
        Set(value As String)
            Application.Current.Resources("L_HOME") = value
        End Set
    End Property
    Public Shared Property CURRENTLYWATCHING As String
        Get
            Return Application.Current.Resources("L_CURRENTLYWATCHING")
        End Get
        Set(value As String)
            Application.Current.Resources("L_CURRENTLYWATCHING") = value
        End Set
    End Property
    Public Shared Property RECENTACTIVITY As String
        Get
            Return Application.Current.Resources("L_RECENTACTIVITY")
        End Get
        Set(value As String)
            Application.Current.Resources("L_RECENTACTIVITY") = value
        End Set
    End Property
    Public Shared Property COLLECTION As String
        Get
            Return Application.Current.Resources("L_COLLECTION")
        End Get
        Set(value As String)
            Application.Current.Resources("L_COLLECTION") = value
        End Set
    End Property
    Public Shared Property LISTS As String
        Get
            Return Application.Current.Resources("L_LISTS")
        End Get
        Set(value As String)
            Application.Current.Resources("L_LISTS") = value
        End Set
    End Property
    Public Shared Property ALL As String
        Get
            Return Application.Current.Resources("L_ALL")
        End Get
        Set(value As String)
            Application.Current.Resources("L_ALL") = value
        End Set
    End Property
    Public Shared Property WATCHING As String
        Get
            Return Application.Current.Resources("L_WATCHING")
        End Get
        Set(value As String)
            Application.Current.Resources("L_WATCHING") = value
        End Set
    End Property
    Public Shared Property COMPLETED As String
        Get
            Return Application.Current.Resources("L_COMPLETED")
        End Get
        Set(value As String)
            Application.Current.Resources("L_COMPLETED") = value
        End Set
    End Property
    Public Shared Property PAUSED As String
        Get
            Return Application.Current.Resources("L_PAUSED")
        End Get
        Set(value As String)
            Application.Current.Resources("L_PAUSED") = value
        End Set
    End Property
    Public Shared Property DROPPED As String
        Get
            Return Application.Current.Resources("L_DROPPED")
        End Get
        Set(value As String)
            Application.Current.Resources("L_DROPPED") = value
        End Set
    End Property
    Public Shared Property PLANNING As String
        Get
            Return Application.Current.Resources("L_PLANNING")
        End Get
        Set(value As String)
            Application.Current.Resources("L_PLANNING") = value
        End Set
    End Property
    Public Shared Property TITLE As String
        Get
            Return Application.Current.Resources("L_TITLE")
        End Get
        Set(value As String)
            Application.Current.Resources("L_TITLE") = value
        End Set
    End Property
    Public Shared Property SCORE As String
        Get
            Return Application.Current.Resources("L_SCORE")
        End Get
        Set(value As String)
            Application.Current.Resources("L_SCORE") = value
        End Set
    End Property
    Public Shared Property PROGRESS As String
        Get
            Return Application.Current.Resources("L_PROGRESS")
        End Get
        Set(value As String)
            Application.Current.Resources("L_PROGRESS") = value
        End Set
    End Property
    Public Shared Property BROWSE As String
        Get
            Return Application.Current.Resources("L_BROWSE")
        End Get
        Set(value As String)
            Application.Current.Resources("L_BROWSE") = value
        End Set
    End Property
    Public Shared Property HOTANIME As String
        Get
            Return Application.Current.Resources("L_HOTANIME")
        End Get
        Set(value As String)
            Application.Current.Resources("L_HOTANIME") = value
        End Set
    End Property
    Public Shared Property RANDOMRECOMMENDATIONS As String
        Get
            Return Application.Current.Resources("L_RANDOMRECOMMENDATIONS")
        End Get
        Set(value As String)
            Application.Current.Resources("L_RANDOMRECOMMENDATIONS") = value
        End Set
    End Property
    Public Shared Property OFFLINEBROWSING As String
        Get
            Return Application.Current.Resources("L_OFFLINEBROWSING")
        End Get
        Set(value As String)
            Application.Current.Resources("L_OFFLINEBROWSING") = value
        End Set
    End Property
    Public Shared Property LOCALCOLLECTION As String
        Get
            Return Application.Current.Resources("L_LOCALCOLLECTION")
        End Get
        Set(value As String)
            Application.Current.Resources("L_LOCALCOLLECTION") = value
        End Set
    End Property
    Public Shared Property GENERAL As String
        Get
            Return Application.Current.Resources("L_GENERAL")
        End Get
        Set(value As String)
            Application.Current.Resources("L_GENERAL") = value
        End Set
    End Property
    Public Shared Property CLIENT As String
        Get
            Return Application.Current.Resources("L_CLIENT")
        End Get
        Set(value As String)
            Application.Current.Resources("L_CLIENT") = value
        End Set
    End Property
    Public Shared Property LOADCHARACTERS As String
        Get
            Return Application.Current.Resources("L_LOADCHARACTERS")
        End Get
        Set(value As String)
            Application.Current.Resources("L_LOADCHARACTERS") = value
        End Set
    End Property
    Public Shared Property LOADCHARACTERSPICTURES As String
        Get
            Return Application.Current.Resources("L_LOADCHARACTERSPICTURES")
        End Get
        Set(value As String)
            Application.Current.Resources("L_LOADCHARACTERSPICTURES") = value
        End Set
    End Property
    Public Shared Property LOADANIMEPICTURES As String
        Get
            Return Application.Current.Resources("L_LOADANIMEPICTURES")
        End Get
        Set(value As String)
            Application.Current.Resources("L_LOADANIMEPICTURES") = value
        End Set
    End Property
    Public Shared Property CACHESETTINGS As String
        Get
            Return Application.Current.Resources("L_CACHESETTINGS")
        End Get
        Set(value As String)
            Application.Current.Resources("L_CACHESETTINGS") = value
        End Set
    End Property
    Public Shared Property DISTINCTCACHE As String
        Get
            Return Application.Current.Resources("L_DISTINCTCACHE")
        End Get
        Set(value As String)
            Application.Current.Resources("L_DISTINCTCACHE") = value
        End Set
    End Property
    Public Shared Property CLEARCACHE As String
        Get
            Return Application.Current.Resources("L_CLEARCACHE")
        End Get
        Set(value As String)
            Application.Current.Resources("L_CLEARCACHE") = value
        End Set
    End Property
    Public Shared Property CLEARIMAGECACHE As String
        Get
            Return Application.Current.Resources("L_CLEARIMAGECACHE")
        End Get
        Set(value As String)
            Application.Current.Resources("L_CLEARIMAGECACHE") = value
        End Set
    End Property
    Public Shared Property ABOUT As String
        Get
            Return Application.Current.Resources("L_ABOUT")
        End Get
        Set(value As String)
            Application.Current.Resources("L_ABOUT") = value
        End Set
    End Property
    Public Shared Property SEARCH As String
        Get
            Return Application.Current.Resources("L_SEARCH")
        End Get
        Set(value As String)
            Application.Current.Resources("L_SEARCH") = value
        End Set
    End Property
    Public Shared Property SETAS As String
        Get
            Return Application.Current.Resources("L_SETAS")
        End Get
        Set(value As String)
            Application.Current.Resources("L_SETAS") = value
        End Set
    End Property
    Public Shared Property FORMAT As String
        Get
            Return Application.Current.Resources("L_FORMAT")
        End Get
        Set(value As String)
            Application.Current.Resources("L_FORMAT") = value
        End Set
    End Property
    Public Shared Property EPISODECOUNT As String
        Get
            Return Application.Current.Resources("L_EPISODECOUNT")
        End Get
        Set(value As String)
            Application.Current.Resources("L_EPISODECOUNT") = value
        End Set
    End Property
    Public Shared Property STATUS As String
        Get
            Return Application.Current.Resources("L_STATUS")
        End Get
        Set(value As String)
            Application.Current.Resources("L_STATUS") = value
        End Set
    End Property
    Public Shared Property STARTDATE As String
        Get
            Return Application.Current.Resources("L_STARTDATE")
        End Get
        Set(value As String)
            Application.Current.Resources("L_STARTDATE") = value
        End Set
    End Property
    Public Shared Property ENDDATE As String
        Get
            Return Application.Current.Resources("L_ENDDATE")
        End Get
        Set(value As String)
            Application.Current.Resources("L_ENDDATE") = value
        End Set
    End Property
    Public Shared Property SEASON As String
        Get
            Return Application.Current.Resources("L_SEASON")
        End Get
        Set(value As String)
            Application.Current.Resources("L_SEASON") = value
        End Set
    End Property
    Public Shared Property CHARACTERS As String
        Get
            Return Application.Current.Resources("L_CHARACTERS")
        End Get
        Set(value As String)
            Application.Current.Resources("L_CHARACTERS") = value
        End Set
    End Property
    Public Shared Property CREATORS As String
        Get
            Return Application.Current.Resources("L_CREATORS")
        End Get
        Set(value As String)
            Application.Current.Resources("L_CREATORS") = value
        End Set
    End Property
    Public Shared Property NAME As String
        Get
            Return Application.Current.Resources("L_NAME")
        End Get
        Set(value As String)
            Application.Current.Resources("L_NAME") = value
        End Set
    End Property
    Public Shared Property CREDIT As String
        Get
            Return Application.Current.Resources("L_CREDIT")
        End Get
        Set(value As String)
            Application.Current.Resources("L_CREDIT") = value
        End Set
    End Property
    Public Shared Property TITLES As String
        Get
            Return Application.Current.Resources("L_TITLES")
        End Get
        Set(value As String)
            Application.Current.Resources("L_TITLES") = value
        End Set
    End Property
    Public Shared Property TYPE As String
        Get
            Return Application.Current.Resources("L_TYPE")
        End Get
        Set(value As String)
            Application.Current.Resources("L_TYPE") = value
        End Set
    End Property
    Public Shared Property LANGUAGE As String
        Get
            Return Application.Current.Resources("L_LANGUAGE")
        End Get
        Set(value As String)
            Application.Current.Resources("L_LANGUAGE") = value
        End Set
    End Property
    Public Shared Property RELATEDANIMES As String
        Get
            Return Application.Current.Resources("L_RELATEDANIMES")
        End Get
        Set(value As String)
            Application.Current.Resources("L_RELATEDANIMES") = value
        End Set
    End Property
    Public Shared Property APPROVAL As String
        Get
            Return Application.Current.Resources("L_APPROVAL")
        End Get
        Set(value As String)
            Application.Current.Resources("L_APPROVAL") = value
        End Set
    End Property
    Public Shared Property WINTER As String
        Get
            Return Application.Current.Resources("L_WINTER")
        End Get
        Set(value As String)
            Application.Current.Resources("L_WINTER") = value
        End Set
    End Property
    Public Shared Property SUMMER As String
        Get
            Return Application.Current.Resources("L_SUMMER")
        End Get
        Set(value As String)
            Application.Current.Resources("L_SUMMER") = value
        End Set
    End Property
    Public Shared Property SPRING As String
        Get
            Return Application.Current.Resources("L_SPRING")
        End Get
        Set(value As String)
            Application.Current.Resources("L_SPRING") = value
        End Set
    End Property
    Public Shared Property FALL As String
        Get
            Return Application.Current.Resources("L_FALL")
        End Get
        Set(value As String)
            Application.Current.Resources("L_FALL") = value
        End Set
    End Property
    Public Shared Property UPDATECACHE As String
        Get
            Return Application.Current.Resources("L_UPDATECACHE")
        End Get
        Set(value As String)
            Application.Current.Resources("L_UPDATECACHE") = value
        End Set
    End Property
    Public Shared Property SETDATALOCATION As String
        Get
            Return Application.Current.Resources("L_SETDATALOCATION")
        End Get
        Set(value As String)
            Application.Current.Resources("L_SETDATALOCATION") = value
        End Set
    End Property
    Public Shared Property THEME As String
        Get
            Return Application.Current.Resources("L_THEME")
        End Get
        Set(value As String)
            Application.Current.Resources("L_THEME") = value
        End Set
    End Property
    Public Shared Property ADDED As String
        Get
            Return Application.Current.Resources("L_ADDED")
        End Get
        Set(value As String)
            Application.Current.Resources("L_ADDED") = value
        End Set
    End Property
    Public Shared Property [TO] As String
        Get
            Return Application.Current.Resources("L_TO")
        End Get
        Set(value As String)
            Application.Current.Resources("L_TO") = value
        End Set
    End Property
    Public Shared Property REMOVED As String
        Get
            Return Application.Current.Resources("L_REMOVED")
        End Get
        Set(value As String)
            Application.Current.Resources("L_REMOVED") = value
        End Set
    End Property
    Public Shared Property FROM As String
        Get
            Return Application.Current.Resources("L_FROM")
        End Get
        Set(value As String)
            Application.Current.Resources("L_FROM") = value
        End Set
    End Property
    Public Shared Property MOVED As String
        Get
            Return Application.Current.Resources("L_MOVED")
        End Get
        Set(value As String)
            Application.Current.Resources("L_MOVED") = value
        End Set
    End Property
    Public Shared Property UPDATED As String
        Get
            Return Application.Current.Resources("L_UPDATED")
        End Get
        Set(value As String)
            Application.Current.Resources("L_UPDATED") = value
        End Set
    End Property
    Public Shared Property WATCHED As String
        Get
            Return Application.Current.Resources("L_WATCHED")
        End Get
        Set(value As String)
            Application.Current.Resources("L_WATCHED") = value
        End Set
    End Property
    Public Shared Property EPISODE As String
        Get
            Return Application.Current.Resources("L_EPISODE")
        End Get
        Set(value As String)
            Application.Current.Resources("L_EPISODE") = value
        End Set
    End Property
    Public Shared Property [OF] As String
        Get
            Return Application.Current.Resources("L_OF")
        End Get
        Set(value As String)
            Application.Current.Resources("L_OF") = value
        End Set
    End Property
    Public Shared Property NOTAVAILABLE As String
        Get
            Return Application.Current.Resources("L_NOTAVAILABLE")
        End Get
        Set(value As String)
            Application.Current.Resources("L_NOTAVAILABLE") = value
        End Set
    End Property
    Public Shared Property EP As String
        Get
            Return Application.Current.Resources("L_EP")
        End Get
        Set(value As String)
            Application.Current.Resources("L_EP") = value
        End Set
    End Property
    Public Shared Property DATA As String
        Get
            Return Application.Current.Resources("L_DATA")
        End Get
        Set(value As String)
            Application.Current.Resources("L_DATA") = value
        End Set
    End Property
    Public Shared Property RETRIEVING As String
        Get
            Return Application.Current.Resources("L_RETRIEVING")
        End Get
        Set(value As String)
            Application.Current.Resources("L_RETRIEVING") = value
        End Set
    End Property
    Public Shared Property FAILED As String
        Get
            Return Application.Current.Resources("L_FAILED")
        End Get
        Set(value As String)
            Application.Current.Resources("L_FAILED") = value
        End Set
    End Property
    Public Shared Property RETRYING As String
        Get
            Return Application.Current.Resources("L_RETRYING")
        End Get
        Set(value As String)
            Application.Current.Resources("L_RETRYING") = value
        End Set
    End Property
    Public Shared Property ABORTING As String
        Get
            Return Application.Current.Resources("L_ABORTING")
        End Get
        Set(value As String)
            Application.Current.Resources("L_ABORTING") = value
        End Set
    End Property
    Public Shared Property [FOR] As String
        Get
            Return Application.Current.Resources("L_FOR")
        End Get
        Set(value As String)
            Application.Current.Resources("L_FOR") = value
        End Set
    End Property
    Public Shared Property FETCHING As String
        Get
            Return Application.Current.Resources("L_FETCHING")
        End Get
        Set(value As String)
            Application.Current.Resources("L_FETCHING") = value
        End Set
    End Property
    Public Shared Property SAVE As String
        Get
            Return Application.Current.Resources("L_SAVE")
        End Get
        Set(value As String)
            Application.Current.Resources("L_SAVE") = value
        End Set
    End Property
    Public Shared Property CLOSE As String
        Get
            Return Application.Current.Resources("L_CLOSE")
        End Get
        Set(value As String)
            Application.Current.Resources("L_CLOSE") = value
        End Set
    End Property
    Public Shared Property EPISODEPROGRESS As String
        Get
            Return Application.Current.Resources("L_EPISODEPROGRESS")
        End Get
        Set(value As String)
            Application.Current.Resources("L_EPISODEPROGRESS") = value
        End Set
    End Property
    Public Shared Property NOTE As String
        Get
            Return Application.Current.Resources("L_NOTE")
        End Get
        Set(value As String)
            Application.Current.Resources("L_NOTE") = value
        End Set
    End Property
    Public Shared Property WALKTHROUGH As String
        Get
            Return Application.Current.Resources("L_WALKTHROUGH")
        End Get
        Set(value As String)
            Application.Current.Resources("L_WALKTHROUGH") = value
        End Set
    End Property
    Public Shared Property LIBRARYPATHS As String
        Get
            Return Application.Current.Resources("L_LIBRARYPATHS")
        End Get
        Set(value As String)
            Application.Current.Resources("L_LIBRARYPATHS") = value
        End Set
    End Property
    Public Shared Property ADD As String
        Get
            Return Application.Current.Resources("L_ADD")
        End Get
        Set(value As String)
            Application.Current.Resources("L_ADD") = value
        End Set
    End Property
    Public Shared Property [NEXT] As String
        Get
            Return Application.Current.Resources("L_NEXT")
        End Get
        Set(value As String)
            Application.Current.Resources("L_NEXT") = value
        End Set
    End Property
    Public Shared Property PREVIOUS As String
        Get
            Return Application.Current.Resources("L_PREVIOUS")
        End Get
        Set(value As String)
            Application.Current.Resources("L_PREVIOUS") = value
        End Set
    End Property
    Public Shared Property [SET] As String
        Get
            Return Application.Current.Resources("L_SET")
        End Get
        Set(value As String)
            Application.Current.Resources("L_SET") = value
        End Set
    End Property
    Public Shared Property FILEEXISTS As String
        Get
            Return Application.Current.Resources("L_FILEEXISTS")
        End Get
        Set(value As String)
            Application.Current.Resources("L_FILEEXISTS") = value
        End Set
    End Property
    Public Shared Property LIBRARY As String
        Get
            Return Application.Current.Resources("L_LIBRARY")
        End Get
        Set(value As String)
            Application.Current.Resources("L_LIBRARY") = value
        End Set
    End Property
    Public Shared Property CACHE As String
        Get
            Return Application.Current.Resources("L_CACHE")
        End Get
        Set(value As String)
            Application.Current.Resources("L_CACHE") = value
        End Set
    End Property
    Public Shared Property OVERWRITE As String
        Get
            Return Application.Current.Resources("L_OVERWRITE")
        End Get
        Set(value As String)
            Application.Current.Resources("L_OVERWRITE") = value
        End Set
    End Property
    Public Shared Property OK As String
        Get
            Return Application.Current.Resources("L_OK")
        End Get
        Set(value As String)
            Application.Current.Resources("L_OK") = value
        End Set
    End Property
    Public Shared Property DOWNLOADMANUALLY As String
        Get
            Return Application.Current.Resources("L_DOWNLOADMANUALLY")
        End Get
        Set(value As String)
            Application.Current.Resources("L_DOWNLOADMANUALLY") = value
        End Set
    End Property
    Public Shared Property SEARCHDISABLED As String
        Get
            Return Application.Current.Resources("L_SEARCHDISABLED")
        End Get
        Set(value As String)
            Application.Current.Resources("L_SEARCHDISABLED") = value
        End Set
    End Property
    Public Shared Property ANIME As String
        Get
            Return Application.Current.Resources("L_ANIME")
        End Get
        Set(value As String)
            Application.Current.Resources("L_ANIME") = value
        End Set
    End Property
    Public Shared Property RETRIEVINGANIMEDATA As String
        Get
            Return Application.Current.Resources("L_RETRIEVINGANIMEDATA")
        End Get
        Set(value As String)
            Application.Current.Resources("L_RETRIEVINGANIMEDATA") = value
        End Set
    End Property
    Public Shared Property CALCULATING As String
        Get
            Return Application.Current.Resources("L_CALCULATING")
        End Get
        Set(value As String)
            Application.Current.Resources("L_CALCULATING") = value
        End Set
    End Property
    Public Shared Property CACHESIZE As String
        Get
            Return Application.Current.Resources("L_CACHESIZE")
        End Get
        Set(value As String)
            Application.Current.Resources("L_CACHESIZE") = value
        End Set
    End Property
    Public Shared Property CACHELIFE As String
        Get
            Return Application.Current.Resources("L_CACHELIFE")
        End Get
        Set(value As String)
            Application.Current.Resources("L_CACHELIFE") = value
        End Set
    End Property
    Public Shared Property CONNECTINGITEMS As String
        Get
            Return Application.Current.Resources("L_CONNECTINGITEMS")
        End Get
        Set(value As String)
            Application.Current.Resources("L_CONNECTINGITEMS") = value
        End Set
    End Property
    Public Shared Property FETCHINGLIBRARYDATA As String
        Get
            Return Application.Current.Resources("L_FETCHINGLIBRARYDATA")
        End Get
        Set(value As String)
            Application.Current.Resources("L_FETCHINGLIBRARYDATA") = value
        End Set
    End Property
    Public Shared Property FETCHINGCOMPLETEDANIMES As String
        Get
            Return Application.Current.Resources("L_FETCHINGCOMPLETEDANIMES")
        End Get
        Set(value As String)
            Application.Current.Resources("L_FETCHINGCOMPLETEDANIMES") = value
        End Set
    End Property
    Public Shared Property FETCHINGPAUSEDANIMES As String
        Get
            Return Application.Current.Resources("L_FETCHINGPAUSEDANIMES")
        End Get
        Set(value As String)
            Application.Current.Resources("L_FETCHINGPAUSEDANIMES") = value
        End Set
    End Property
    Public Shared Property FETCHINGDROPPEDANIMES As String
        Get
            Return Application.Current.Resources("L_FETCHINGDROPPEDANIMES")
        End Get
        Set(value As String)
            Application.Current.Resources("L_FETCHINGDROPPEDANIMES") = value
        End Set
    End Property
    Public Shared Property FETCHINGPLANNEDANIMES As String
        Get
            Return Application.Current.Resources("L_FETCHINGPLANNEDANIMES")
        End Get
        Set(value As String)
            Application.Current.Resources("L_FETCHINGPLANNEDANIMES") = value
        End Set
    End Property
    Public Shared Property FETCHINGRECENTACTIVITES As String
        Get
            Return Application.Current.Resources("L_FETCHINGRECENTACTIVITES")
        End Get
        Set(value As String)
            Application.Current.Resources("L_FETCHINGRECENTACTIVITES") = value
        End Set
    End Property
    Public Shared Property FETCHINGCACHEDANIMES As String
        Get
            Return Application.Current.Resources("L_FETCHINGCACHEDANIMES")
        End Get
        Set(value As String)
            Application.Current.Resources("L_FETCHINGCACHEDANIMES") = value
        End Set
    End Property
    Public Shared Property FETCHINGLOCALCOLLECTION As String
        Get
            Return Application.Current.Resources("L_FETCHINGLOCALCOLLECTION")
        End Get
        Set(value As String)
            Application.Current.Resources("L_FETCHINGLOCALCOLLECTION") = value
        End Set
    End Property
    Public Shared Property BY As String
        Get
            Return Application.Current.Resources("L_BY")
        End Get
        Set(value As String)
            Application.Current.Resources("L_BY") = value
        End Set
    End Property
    Public Shared Property USER As String
        Get
            Return Application.Current.Resources("L_USER")
        End Get
        Set(value As String)
            Application.Current.Resources("L_USER") = value
        End Set
    End Property
    Public Shared Property AIRING As String
        Get
            Return Application.Current.Resources("L_AIRING")
        End Get
        Set(value As String)
            Application.Current.Resources("L_AIRING") = value
        End Set
    End Property
    Public Shared Property FINISHED As String
        Get
            Return Application.Current.Resources("L_FINISHED")
        End Get
        Set(value As String)
            Application.Current.Resources("L_FINISHED") = value
        End Set
    End Property
    Public Shared Property SEARCHING As String
        Get
            Return Application.Current.Resources("L_SEARCHING")
        End Get
        Set(value As String)
            Application.Current.Resources("L_SEARCHING") = value
        End Set
    End Property
    Public Shared Property FOUND As String
        Get
            Return Application.Current.Resources("L_FOUND")
        End Get
        Set(value As String)
            Application.Current.Resources("L_FOUND") = value
        End Set
    End Property
    Public Shared Property CACHING As String
        Get
            Return Application.Current.Resources("L_CACHING")
        End Get
        Set(value As String)
            Application.Current.Resources("L_CACHING") = value
        End Set
    End Property
    Public Shared Property LOADFROMFILE As String
        Get
            Return Application.Current.Resources("L_LOADFROMFILE")
        End Get
        Set(value As String)
            Application.Current.Resources("L_LOADFROMFILE") = value
        End Set
    End Property
    Public Shared Property SEARCHMISSING As String
        Get
            Return Application.Current.Resources("L_SEARCHMISSING")
        End Get
        Set(value As String)
            Application.Current.Resources("L_SEARCHMISSING") = value
        End Set
    End Property
    Public Shared Property SEARCHMISSINGNOTE As String
        Get
            Return Application.Current.Resources("L_SEARCHMISSINGNOTE")
        End Get
        Set(value As String)
            Application.Current.Resources("L_SEARCHMISSINGNOTE") = value
        End Set
    End Property
    Public Shared Property LOADING As String
        Get
            Return Application.Current.Resources("L_LOADING")
        End Get
        Set(value As String)
            Application.Current.Resources("L_LOADING") = value
        End Set
    End Property
    Public Shared Property SEARCHNOTRECOGNIZED As String
        Get
            Return Application.Current.Resources("L_SEARCHNOTRECOGNIZED")
        End Get
        Set(value As String)
            Application.Current.Resources("L_SEARCHNOTRECOGNIZED") = value
        End Set
    End Property
    Public Shared Property DOWNLOADING As String
        Get
            Return Application.Current.Resources("L_DOWNLOADING")
        End Get
        Set(value As String)
            Application.Current.Resources("L_DOWNLOADING") = value
        End Set
    End Property
    Public Shared Property SEARCHNOTREADING As String
        Get
            Return Application.Current.Resources("L_SEARCHNOTREADING")
        End Get
        Set(value As String)
            Application.Current.Resources("L_SEARCHNOTREADING") = value
        End Set
    End Property
    Public Shared Property [ERROR] As String
        Get
            Return Application.Current.Resources("L_ERROR")
        End Get
        Set(value As String)
            Application.Current.Resources("L_ERROR") = value
        End Set
    End Property
    Public Shared Property WITHVALUE As String
        Get
            Return Application.Current.Resources("L_WITHVALUE")
        End Get
        Set(value As String)
            Application.Current.Resources("L_WITHVALUE") = value
        End Set
    End Property
    Public Shared Property DELETEWARNING As String
        Get
            Return Application.Current.Resources("L_DELETEWARNING")
        End Get
        Set(value As String)
            Application.Current.Resources("L_DELETEWARNING") = value
        End Set
    End Property
    Public Shared Property ANIMEFILES As String
        Get
            Return Application.Current.Resources("L_ANIMEFILES")
        End Get
        Set(value As String)
            Application.Current.Resources("L_ANIMEFILES") = value
        End Set
    End Property
    Public Shared Property THUMBFILES As String
        Get
            Return Application.Current.Resources("L_THUMBFILES")
        End Get
        Set(value As String)
            Application.Current.Resources("L_THUMBFILES") = value
        End Set
    End Property
    Public Shared Property NO As String
        Get
            Return Application.Current.Resources("L_NO")
        End Get
        Set(value As String)
            Application.Current.Resources("L_NO") = value
        End Set
    End Property
    Public Shared Property DELETING As String
        Get
            Return Application.Current.Resources("L_DELETING")
        End Get
        Set(value As String)
            Application.Current.Resources("L_DELETING") = value
        End Set
    End Property
    Public Shared Property DATAWARNING As String
        Get
            Return Application.Current.Resources("L_DATAWARNING")
        End Get
        Set(value As String)
            Application.Current.Resources("L_DATAWARNING") = value
        End Set
    End Property
    Public Shared Property YES As String
        Get
            Return Application.Current.Resources("L_YES")
        End Get
        Set(value As String)
            Application.Current.Resources("L_YES") = value
        End Set
    End Property
    Public Shared Property MOVEWARNING As String
        Get
            Return Application.Current.Resources("L_MOVEWARNING")
        End Get
        Set(value As String)
            Application.Current.Resources("L_MOVEWARNING") = value
        End Set
    End Property
    Public Shared Property UPDATING As String
        Get
            Return Application.Current.Resources("L_UPDATING")
        End Get
        Set(value As String)
            Application.Current.Resources("L_UPDATING") = value
        End Set
    End Property
    Public Shared Property ITEMS As String
        Get
            Return Application.Current.Resources("L_ITEMS")
        End Get
        Set(value As String)
            Application.Current.Resources("L_ITEMS") = value
        End Set
    End Property
    Public Shared Property GENDER As String
        Get
            Return Application.Current.Resources("L_GENDER")
        End Get
        Set(value As String)
            Application.Current.Resources("L_GENDER") = value
        End Set
    End Property
    Public Shared Property SETTINGS_S As String
        Get
            Return Application.Current.Resources("L_SETTINGS")
        End Get
        Set(value As String)
            Application.Current.Resources("L_SETTINGS") = value
        End Set
    End Property
    Public Shared Property SEARCHITEMS As String
        Get
            Return Application.Current.Resources("L_SEARCHITEMS")
        End Get
        Set(value As String)
            Application.Current.Resources("L_SEARCHITEMS") = value
        End Set
    End Property
    Public Shared Property THEMEMAKER As String
        Get
            Return Application.Current.Resources("L_THEMEMAKER")
        End Get
        Set(value As String)
            Application.Current.Resources("L_THEMEMAKER") = value
        End Set
    End Property
    Public Shared Property TOPBAR_S As String
        Get
            Return Application.Current.Resources("L_TOPBAR")
        End Get
        Set(value As String)
            Application.Current.Resources("L_TOPBAR") = value
        End Set
    End Property
    Public Shared Property TOPBARTEXT_S As String
        Get
            Return Application.Current.Resources("L_TOPBARTEXT")
        End Get
        Set(value As String)
            Application.Current.Resources("L_TOPBARTEXT") = value
        End Set
    End Property
    Public Shared Property BACKGROUND As String
        Get
            Return Application.Current.Resources("L_BACKGROUND")
        End Get
        Set(value As String)
            Application.Current.Resources("L_BACKGROUND") = value
        End Set
    End Property
    Public Shared Property CONTENT_S As String
        Get
            Return Application.Current.Resources("L_CONTENT")
        End Get
        Set(value As String)
            Application.Current.Resources("L_CONTENT") = value
        End Set
    End Property
    Public Shared Property ACCENT_S As String
        Get
            Return Application.Current.Resources("L_ACCENT")
        End Get
        Set(value As String)
            Application.Current.Resources("L_ACCENT") = value
        End Set
    End Property
    Public Shared Property OVERLAY_S As String
        Get
            Return Application.Current.Resources("L_OVERLAY")
        End Get
        Set(value As String)
            Application.Current.Resources("L_OVERLAY") = value
        End Set
    End Property
    Public Shared Property OVERLAYTEXT_S As String
        Get
            Return Application.Current.Resources("L_OVERLAYTEXT")
        End Get
        Set(value As String)
            Application.Current.Resources("L_OVERLAYTEXT") = value
        End Set
    End Property
    Public Shared Property TEXT_S As String
        Get
            Return Application.Current.Resources("L_TEXT")
        End Get
        Set(value As String)
            Application.Current.Resources("L_TEXT") = value
        End Set
    End Property
    Public Shared Property FONT_S As String
        Get
            Return Application.Current.Resources("L_FONT")
        End Get
        Set(value As String)
            Application.Current.Resources("L_FONT") = value
        End Set
    End Property
    Public Shared Property FONTWEIGHT_S As String
        Get
            Return Application.Current.Resources("L_FONTWEIGHT")
        End Get
        Set(value As String)
            Application.Current.Resources("L_FONTWEIGHT") = value
        End Set
    End Property
    Public Shared Property THEMEWARNING As String
        Get
            Return Application.Current.Resources("L_THEMEWARNING")
        End Get
        Set(value As String)
            Application.Current.Resources("L_THEMEWARNING") = value
        End Set
    End Property
    Public Shared Property RESTRICTEDCONTENT As String
        Get
            Return Application.Current.Resources("L_RESTRICTEDCONTENT")
        End Get
        Set(value As String)
            Application.Current.Resources("L_RESTRICTEDCONTENT") = value
        End Set
    End Property
    Public Shared Property AUDX As String
        Get
            Return Application.Current.Resources("L_AUDX")
        End Get
        Set(value As String)
            Application.Current.Resources("L_AUDX") = value
        End Set
    End Property
    Public Shared Property SEEIN As String
        Get
            Return Application.Current.Resources("L_SEEIN")
        End Get
        Set(value As String)
            Application.Current.Resources("L_SEEIN") = value
        End Set
    End Property
    Public Shared Property MEMORYOPTIMIZATION As String
        Get
            Return Application.Current.Resources("L_MEMORYOPTIMIZATION")
        End Get
        Set(value As String)
            Application.Current.Resources("L_MEMORYOPTIMIZATION") = value
        End Set
    End Property
    Public Shared Property AUDXWARNING As String
        Get
            Return Application.Current.Resources("L_AUDXWARNING")
        End Get
        Set(value As String)
            Application.Current.Resources("L_AUDXWARNING") = value
        End Set
    End Property
    Public Shared Property OPENING As String
        Get
            Return Application.Current.Resources("L_OPENING")
        End Get
        Set(value As String)
            Application.Current.Resources("L_OPENING") = value
        End Set
    End Property
    Public Shared Property ENDING As String
        Get
            Return Application.Current.Resources("L_ENDING")
        End Get
        Set(value As String)
            Application.Current.Resources("L_ENDING") = value
        End Set
    End Property
    Public Shared Property FULLSCREENWARNING As String
        Get
            Return Application.Current.Resources("L_FULLSCREENWARNING")
        End Get
        Set(value As String)
            Application.Current.Resources("L_FULLSCREENWARNING") = value
        End Set
    End Property
    Public Shared Property SETCLIENT As String
        Get
            Return Application.Current.Resources("L_SETCLIENT")
        End Get
        Set(value As String)
            Application.Current.Resources("L_SETCLIENT") = value
        End Set
    End Property
    Public Shared Property VERSION As String
        Get
            Return Application.Current.Resources("L_VERSION")
        End Get
        Set(value As String)
            Application.Current.Resources("L_VERSION") = value
        End Set
    End Property
    Public Shared Property CREDENTIALSWARNING As String
        Get
            Return Application.Current.Resources("L_CREDENTIALSWARNING")
        End Get
        Set(value As String)
            Application.Current.Resources("L_CREDENTIALSWARNING") = value
        End Set
    End Property
    Public Shared Property BACKGROUNDLOADING As String
        Get
            Return Application.Current.Resources("L_BACKGROUNDLOADING")
        End Get
        Set(value As String)
            Application.Current.Resources("L_BACKGROUNDLOADING") = value
        End Set
    End Property
    Public Shared Property FEATURED As String
        Get
            Return Application.Current.Resources("L_FEATURED")
        End Get
        Set(value As String)
            Application.Current.Resources("L_FEATURED") = value
        End Set
    End Property
    Public Shared Property WAIFU As String
        Get
            Return Application.Current.Resources("L_WAIFU")
        End Get
        Set(value As String)
            Application.Current.Resources("L_WAIFU") = value
        End Set
    End Property
    Public Shared Property SOUNDTRACKS As String
        Get
            Return Application.Current.Resources("L_SOUNDTRACKS")
        End Get
        Set(value As String)
            Application.Current.Resources("L_SOUNDTRACKS") = value
        End Set
    End Property
    Public Shared Property [CONTINUE] As String
        Get
            Return Application.Current.Resources("L_CONTINUE")
        End Get
        Set(value As String)
            Application.Current.Resources("L_CONTINUE") = value
        End Set
    End Property
    Public Shared Property UPDATE_S As String
        Get
            Return Application.Current.Resources("L_UPDATE")
        End Get
        Set(value As String)
            Application.Current.Resources("L_UPDATE") = value
        End Set
    End Property
    Public Shared Property UPDATESCHECK As String
        Get
            Return Application.Current.Resources("L_UPDATESCHECK")
        End Get
        Set(value As String)
            Application.Current.Resources("L_UPDATESCHECK") = value
        End Set
    End Property
    Public Shared Property UPDATESCHECKC As String
        Get
            Return Application.Current.Resources("L_UPDATESCHECKC")
        End Get
        Set(value As String)
            Application.Current.Resources("L_UPDATESCHECKC") = value
        End Set
    End Property
    Public Shared Property UPDATES As String
        Get
            Return Application.Current.Resources("L_UPDATES")
        End Get
        Set(value As String)
            Application.Current.Resources("L_UPDATES") = value
        End Set
    End Property
    Public Shared Property UPDATESCRITICAL As String
        Get
            Return Application.Current.Resources("L_UPDATESCRITICAL")
        End Get
        Set(value As String)
            Application.Current.Resources("L_UPDATESCRITICAL") = value
        End Set
    End Property
    Public Shared Property UPDATESCRITICALWARNING As String
        Get
            Return Application.Current.Resources("L_UPDATESCRITICALWARNING")
        End Get
        Set(value As String)
            Application.Current.Resources("L_UPDATESCRITICALWARNING") = value
        End Set
    End Property
    Public Shared Property UPDATESERROR As String
        Get
            Return Application.Current.Resources("L_UPDATESERROR")
        End Get
        Set(value As String)
            Application.Current.Resources("L_UPDATESERROR") = value
        End Set
    End Property
    Public Shared Property SOCIAL As String
        Get
            Return Application.Current.Resources("L_SOCIAL")
        End Get
        Set(value As String)
            Application.Current.Resources("L_SOCIAL") = value
        End Set
    End Property
    Public Shared Property EXTERNALLINKS As String
        Get
            Return Application.Current.Resources("L_EXTERNALLINKS")
        End Get
        Set(value As String)
            Application.Current.Resources("L_EXTERNALLINKS") = value
        End Set
    End Property
    Public Shared Property WALLPAPER As String
        Get
            Return Application.Current.Resources("L_WALLPAPER")
        End Get
        Set(value As String)
            Application.Current.Resources("L_WALLPAPER") = value
        End Set
    End Property
    Public Shared Property LOCKSCREEN_S As String
        Get
            Return Application.Current.Resources("L_LOCKSCREEN")
        End Get
        Set(value As String)
            Application.Current.Resources("L_LOCKSCREEN") = value
        End Set
    End Property
    Public Shared Property VIEW As String
        Get
            Return Application.Current.Resources("L_VIEW")
        End Get
        Set(value As String)
            Application.Current.Resources("L_VIEW") = value
        End Set
    End Property
    Public Shared Property INIT_NOINTERNET As String
        Get
            Return Application.Current.Resources("L_INIT_NOINTERNET")
        End Get
        Set(value As String)
            Application.Current.Resources("L_INIT_NOINTERNET") = value
        End Set
    End Property
    Public Shared Property EXTERNAL As String
        Get
            Return Application.Current.Resources("L_EXTERNAL")
        End Get
        Set(value As String)
            Application.Current.Resources("L_EXTERNAL") = value
        End Set
    End Property
    Public Shared Property CLEARCUSTOMLOCKWALL As String
        Get
            Return Application.Current.Resources("L_CLEARCUSTOMLOCKWALL")
        End Get
        Set(value As String)
            Application.Current.Resources("L_CLEARCUSTOMLOCKWALL") = value
        End Set
    End Property
    Public Shared Property INVALIDCLIENT As String
        Get
            Return Application.Current.Resources("L_INVALIDCLIENT")
        End Get
        Set(value As String)
            Application.Current.Resources("L_INVALIDCLIENT") = value
        End Set
    End Property
#End Region

#Region "Style"
    Public Shared Property BG As SolidColorBrush
        Get
            Return Application.Current.Resources("BG")
        End Get
        Set(value As SolidColorBrush)
            Application.Current.Resources("BG") = value
        End Set
    End Property
    Public Shared Property TOPBAR As SolidColorBrush
        Get
            Return Application.Current.Resources("TOPBAR")
        End Get
        Set(value As SolidColorBrush)
            Application.Current.Resources("TOPBAR") = value
        End Set
    End Property
    Public Shared Property TOPBARTEXT As SolidColorBrush
        Get
            Return Application.Current.Resources("TOPBARTEXT")
        End Get
        Set(value As SolidColorBrush)
            Application.Current.Resources("TOPBARTEXT") = value
        End Set
    End Property
    Public Shared Property CONTENT As SolidColorBrush
        Get
            Return Application.Current.Resources("CONTENT")
        End Get
        Set(value As SolidColorBrush)
            Application.Current.Resources("CONTENT") = value
        End Set
    End Property
    Public Shared Property ACCENT As SolidColorBrush
        Get
            Return Application.Current.Resources("ACCENT")
        End Get
        Set(value As SolidColorBrush)
            Application.Current.Resources("ACCENT") = value
        End Set
    End Property
    Public Shared Property OVERLAY As SolidColorBrush
        Get
            Return Application.Current.Resources("OVERLAY")
        End Get
        Set(value As SolidColorBrush)
            Application.Current.Resources("OVERLAY") = value
        End Set
    End Property
    Public Shared Property OVERLAYTEXT As SolidColorBrush
        Get
            Return Application.Current.Resources("OVERLAYTEXT")
        End Get
        Set(value As SolidColorBrush)
            Application.Current.Resources("OVERLAYTEXT") = value
        End Set
    End Property
    Public Shared Property TEXT As SolidColorBrush
        Get
            Return Application.Current.Resources("TEXT")
        End Get
        Set(value As SolidColorBrush)
            Application.Current.Resources("TEXT") = value
        End Set
    End Property
    Public Shared Property FONT As FontFamily
        Get
            Return Application.Current.Resources("FONT")
        End Get
        Set(value As FontFamily)
            Application.Current.Resources("FONT") = value
        End Set
    End Property
    Public Shared Property FONT_WEIGHT As FontWeight
        Get
            Return Application.Current.Resources("FONT_WEIGHT")
        End Get
        Set(value As FontWeight)
            Application.Current.Resources("FONT_WEIGHT") = value
        End Set
    End Property
    Public Shared Property SPLITICON As DrawingImage
        Get
            Return Application.Current.Resources("SPLITICON")
        End Get
        Set(value As DrawingImage)
            Application.Current.Resources("SPLITICON") = value
        End Set
    End Property
    Public Shared Property ICON As DrawingImage
        Get
            Return Application.Current.Resources("ICON")
        End Get
        Set(value As DrawingImage)
            Application.Current.Resources("ICON") = value
        End Set
    End Property
    Public Shared Property WARNING As DrawingImage
        Get
            Return Application.Current.Resources("WARNING")
        End Get
        Set(value As DrawingImage)
            Application.Current.Resources("WARNING") = value
        End Set
    End Property
    Public Shared Property LOGO As DrawingImage
        Get
            Return Application.Current.Resources("LOGO")
        End Get
        Set(value As DrawingImage)
            Application.Current.Resources("LOGO") = value
        End Set
    End Property
    Public Shared Property TRENDING As DrawingImage
        Get
            Return Application.Current.Resources("TRENDING")
        End Get
        Set(value As DrawingImage)
            Application.Current.Resources("TRENDING") = value
        End Set
    End Property
    Public Shared Property ANIMEBOY As DrawingImage
        Get
            Return Application.Current.Resources("ANIMEBOY")
        End Get
        Set(value As DrawingImage)
            Application.Current.Resources("ANIMEBOY") = value
        End Set
    End Property
    Public Shared Property PLAY As DrawingImage
        Get
            Return Application.Current.Resources("PLAY")
        End Get
        Set(value As DrawingImage)
            Application.Current.Resources("PLAY") = value
        End Set
    End Property
    Public Shared Property CHECKMARK As DrawingImage
        Get
            Return Application.Current.Resources("CHECKMARK")
        End Get
        Set(value As DrawingImage)
            Application.Current.Resources("CHECKMARK") = value
        End Set
    End Property
    Public Shared Property PLAN As DrawingImage
        Get
            Return Application.Current.Resources("PLAN")
        End Get
        Set(value As DrawingImage)
            Application.Current.Resources("PLAN") = value
        End Set
    End Property
    Public Shared Property PAUSE As DrawingImage
        Get
            Return Application.Current.Resources("PAUSE")
        End Get
        Set(value As DrawingImage)
            Application.Current.Resources("PAUSE") = value
        End Set
    End Property
    Public Shared Property [STOP] As DrawingImage
        Get
            Return Application.Current.Resources("STOP")
        End Get
        Set(value As DrawingImage)
            Application.Current.Resources("STOP") = value
        End Set
    End Property
    Public Shared Property RATING As DrawingImage
        Get
            Return Application.Current.Resources("RATING")
        End Get
        Set(value As DrawingImage)
            Application.Current.Resources("RATING") = value
        End Set
    End Property
    Public Shared Property CLOCK As DrawingImage
        Get
            Return Application.Current.Resources("CLOCK")
        End Get
        Set(value As DrawingImage)
            Application.Current.Resources("CLOCK") = value
        End Set
    End Property
    Public Shared Property MALE As DrawingImage
        Get
            Return Application.Current.Resources("MALE")
        End Get
        Set(value As DrawingImage)
            Application.Current.Resources("MALE") = value
        End Set
    End Property
    Public Shared Property FEMALE As DrawingImage
        Get
            Return Application.Current.Resources("FEMALE")
        End Get
        Set(value As DrawingImage)
            Application.Current.Resources("FEMALE") = value
        End Set
    End Property
    Public Shared Property EDIT As DrawingImage
        Get
            Return Application.Current.Resources("EDIT")
        End Get
        Set(value As DrawingImage)
            Application.Current.Resources("EDIT") = value
        End Set
    End Property
    Public Shared Property FULLVIEW As DrawingImage
        Get
            Return Application.Current.Resources("FULLVIEW")
        End Get
        Set(value As DrawingImage)
            Application.Current.Resources("FULLVIEW") = value
        End Set
    End Property
    Public Shared Property CROSS As DrawingImage
        Get
            Return Application.Current.Resources("CROSS")
        End Get
        Set(value As DrawingImage)
            Application.Current.Resources("CROSS") = value
        End Set
    End Property
    Public Shared Property PLUS As DrawingImage
        Get
            Return Application.Current.Resources("PLUS")
        End Get
        Set(value As DrawingImage)
            Application.Current.Resources("PLUS") = value
        End Set
    End Property
    Public Shared Property DRIVE As DrawingImage
        Get
            Return Application.Current.Resources("DRIVE")
        End Get
        Set(value As DrawingImage)
            Application.Current.Resources("DRIVE") = value
        End Set
    End Property
    Public Shared Property SETTINGS As DrawingImage
        Get
            Return Application.Current.Resources("SETTINGS")
        End Get
        Set(value As DrawingImage)
            Application.Current.Resources("SETTINGS") = value
        End Set
    End Property
    Public Shared Property CLOUD As DrawingImage
        Get
            Return Application.Current.Resources("CLOUD")
        End Get
        Set(value As DrawingImage)
            Application.Current.Resources("CLOUD") = value
        End Set
    End Property
    Public Shared Property DATABASE As DrawingImage
        Get
            Return Application.Current.Resources("DATABASE")
        End Get
        Set(value As DrawingImage)
            Application.Current.Resources("DATABASE") = value
        End Set
    End Property
    Public Shared Property APPTHEME As Integer
        Get
            Return Application.Current.Resources("APPTHEME")
        End Get
        Set(value As Integer)
            Application.Current.Resources("APPTHEME") = value
        End Set
    End Property
    Public Shared Property BG_PATH As String
        Get
            Return Application.Current.Resources("BG_PATH")
        End Get
        Set(value As String)
            Application.Current.Resources("BG_PATH") = value
        End Set
    End Property
    Public Shared Property BG_ISPATH As Boolean
        Get
            Return Application.Current.Resources("BG_ISPATH")
        End Get
        Set(value As Boolean)
            Application.Current.Resources("BG_ISPATH") = value
        End Set
    End Property
    Public Shared Property BG_ISPATHRELATIVE As Boolean
        Get
            Return Application.Current.Resources("BG_ISPATHRELATIVE")
        End Get
        Set(value As Boolean)
            Application.Current.Resources("BG_ISPATHRELATIVE") = value
        End Set
    End Property
    Public Shared Property BG_ISPATHINTERNAL As Boolean
        Get
            Return Application.Current.Resources("BG_ISPATHINTERNAL")
        End Get
        Set(value As Boolean)
            Application.Current.Resources("BG_ISPATHINTERNAL") = value
        End Set
    End Property
    Public Shared Property NONET As DrawingImage
        Get
            Return Application.Current.Resources("NONET")
        End Get
        Set(value As DrawingImage)
            Application.Current.Resources("NONET") = value
        End Set
    End Property
    Public Shared Property CONTENT_MARGIN As Thickness
        Get
            Return Application.Current.Resources("CONTENT_MARGIN")
        End Get
        Set(value As Thickness)
            Application.Current.Resources("CONTENT_MARGIN") = value
        End Set
    End Property
    Public Shared Property MAIL As DrawingImage
        Get
            Return Application.Current.Resources("MAIL")
        End Get
        Set(value As DrawingImage)
            Application.Current.Resources("MAIL") = value
        End Set
    End Property
    Public Shared Property UPDATE As DrawingImage
        Get
            Return Application.Current.Resources("UPDATE")
        End Get
        Set(value As DrawingImage)
            Application.Current.Resources("UPDATE") = value
        End Set
    End Property
    Public Shared Property DISCORD As DrawingImage
        Get
            Return Application.Current.Resources("DISCORD")
        End Get
        Set(value As DrawingImage)
            Application.Current.Resources("DISCORD") = value
        End Set
    End Property
    Public Shared Property SOURCEFORGE As DrawingImage
        Get
            Return Application.Current.Resources("SOURCEFORGE")
        End Get
        Set(value As DrawingImage)
            Application.Current.Resources("SOURCEFORGE") = value
        End Set
    End Property
    Public Shared Property GITHUB As DrawingImage
        Get
            Return Application.Current.Resources("GITHUB")
        End Get
        Set(value As DrawingImage)
            Application.Current.Resources("GITHUB") = value
        End Set
    End Property
    Public Shared Property SOCIALLINK As DrawingImage
        Get
            Return Application.Current.Resources("SOCIALLINK")
        End Get
        Set(value As DrawingImage)
            Application.Current.Resources("SOCIALLINK") = value
        End Set
    End Property
    Public Shared Property ANN As DrawingImage
        Get
            Return Application.Current.Resources("ANN")
        End Get
        Set(value As DrawingImage)
            Application.Current.Resources("ANN") = value
        End Set
    End Property
    Public Shared Property MAL As DrawingImage
        Get
            Return Application.Current.Resources("MAL")
        End Get
        Set(value As DrawingImage)
            Application.Current.Resources("MAL") = value
        End Set
    End Property
    Public Shared Property WEBSITE As DrawingImage
        Get
            Return Application.Current.Resources("WEBSITE")
        End Get
        Set(value As DrawingImage)
            Application.Current.Resources("WEBSITE") = value
        End Set
    End Property
    Public Shared Property WIKIPEDIA As DrawingImage
        Get
            Return Application.Current.Resources("WIKIPEDIA")
        End Get
        Set(value As DrawingImage)
            Application.Current.Resources("WIKIPEDIA") = value
        End Set
    End Property
    Public Shared Property SYOBOI As DrawingImage
        Get
            Return Application.Current.Resources("SYOBOI")
        End Get
        Set(value As DrawingImage)
            Application.Current.Resources("SYOBOI") = value
        End Set
    End Property
    Public Shared Property ALLCINEMA As DrawingImage
        Get
            Return Application.Current.Resources("ALLCINEMA")
        End Get
        Set(value As DrawingImage)
            Application.Current.Resources("ALLCINEMA") = value
        End Set
    End Property
    Public Shared Property ANISON As DrawingImage
        Get
            Return Application.Current.Resources("ANISON")
        End Get
        Set(value As DrawingImage)
            Application.Current.Resources("ANISON") = value
        End Set
    End Property
    Public Shared Property DOTLAIN As DrawingImage
        Get
            Return Application.Current.Resources(".LAIN")
        End Get
        Set(value As DrawingImage)
            Application.Current.Resources(".LAIN") = value
        End Set
    End Property
    Public Shared Property VNDB As DrawingImage
        Get
            Return Application.Current.Resources("VNDB")
        End Get
        Set(value As DrawingImage)
            Application.Current.Resources("VNDB") = value
        End Set
    End Property
    Public Shared Property MARUMEGANE As DrawingImage
        Get
            Return Application.Current.Resources("MARUMEGANE")
        End Get
        Set(value As DrawingImage)
            Application.Current.Resources("MARUMEGANE") = value
        End Set
    End Property
    Public Shared Property FACEBOOK As DrawingImage
        Get
            Return Application.Current.Resources("FACEBOOK")
        End Get
        Set(value As DrawingImage)
            Application.Current.Resources("FACEBOOK") = value
        End Set
    End Property
    Public Shared Property TWITTER As DrawingImage
        Get
            Return Application.Current.Resources("TWITTER")
        End Get
        Set(value As DrawingImage)
            Application.Current.Resources("TWITTER") = value
        End Set
    End Property
    Public Shared Property YOUTUBE As DrawingImage
        Get
            Return Application.Current.Resources("YOUTUBE")
        End Get
        Set(value As DrawingImage)
            Application.Current.Resources("YOUTUBE") = value
        End Set
    End Property
    Public Shared Property CRUNCHYROLL As DrawingImage
        Get
            Return Application.Current.Resources("CRUNCHYROLL")
        End Get
        Set(value As DrawingImage)
            Application.Current.Resources("CRUNCHYROLL") = value
        End Set
    End Property
    Public Shared Property AMAZON As DrawingImage
        Get
            Return Application.Current.Resources("AMAZON")
        End Get
        Set(value As DrawingImage)
            Application.Current.Resources("AMAZON") = value
        End Set
    End Property
    Public Shared Property STREAM As DrawingImage
        Get
            Return Application.Current.Resources("STREAM")
        End Get
        Set(value As DrawingImage)
            Application.Current.Resources("STREAM") = value
        End Set
    End Property
    Public Shared Property NETFLIX As DrawingImage
        Get
            Return Application.Current.Resources("NETFLIX")
        End Get
        Set(value As DrawingImage)
            Application.Current.Resources("NETFLIX") = value
        End Set
    End Property
    Public Shared Property LOCKSCREEN As DrawingImage
        Get
            Return Application.Current.Resources("LOCKSCREEN")
        End Get
        Set(value As DrawingImage)
            Application.Current.Resources("LOCKSCREEN") = value
        End Set
    End Property
#End Region
    Public Shared ReadOnly Property LocalizationDictionary As ResourceDictionary
        Get
            Return Application.Current.Resources.MergedDictionaries.FirstOrDefault(Function(Dict) Dict.Item("DICTTYPE") = "LANG")
        End Get
    End Property
    Public Shared ReadOnly Property StyleDictionary As ResourceDictionary
        Get
            Return Application.Current.Resources.MergedDictionaries.FirstOrDefault(Function(Dict) Dict.Item("DICTTYPE") = "STYLE")
        End Get
    End Property
    Public Shared Sub ClearLocalizationDictionaries()
        Dim TBRDicts As New List(Of ResourceDictionary)
        For Each Dict In Application.Current.Resources.MergedDictionaries
            If Not String.IsNullOrEmpty(Dict.Item("DICTTYPE")) Then
                If Dict.Item("DICTTYPE") = "LANG" Then
                    TBRDicts.Add(Dict)
                End If
            End If
        Next
        For Each Dict In TBRDicts
            Application.Current.Resources.MergedDictionaries.Remove(Dict)
        Next
        TBRDicts = Nothing
    End Sub
    Public Shared Sub ClearStyleDictionaries()
        Dim TBRDicts As New List(Of ResourceDictionary)
        For Each Dict In Application.Current.Resources.MergedDictionaries
            If Not String.IsNullOrEmpty(Dict.Item("DICTTYPE")) Then
                If Dict.Item("DICTTYPE") = "STYLE" Then
                    TBRDicts.Add(Dict)
                End If
            End If
        Next
        For Each Dict In TBRDicts
            Application.Current.Resources.MergedDictionaries.Remove(Dict)
        Next
        TBRDicts = Nothing
    End Sub
End Class

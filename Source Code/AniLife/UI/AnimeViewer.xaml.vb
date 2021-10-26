Public Class AnimeViewer
    Public Shadows Property Tag As AniDB.AniDBClient.AnimeElement
    Public Property LibraryControls As Boolean
        Get
            Return If(Library_Controls.Visibility = Visibility.Visible, True, False)
        End Get
        Set(value As Boolean)
            If value Then
                Library_Controls.Visibility = Visibility.Visible
            Else
                Library_Controls.Visibility = Visibility.Collapsed
            End If
        End Set
    End Property
    Private CreatorsLVItems As New ObjectModel.ObservableCollection(Of LVCreatorItem)
    Private RelatedLVItems As New ObjectModel.ObservableCollection(Of LVRelatedItem)
    Private SimilarLVItems As New ObjectModel.ObservableCollection(Of LVSimilarItem)
    Private TitlesLVItems As New ObjectModel.ObservableCollection(Of LVTitleItem)
    Public Sub New(AnimeElement As AniDB.AniDBClient.AnimeElement)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Tag = AnimeElement
        Search_Creators.ItemsSource = CreatorsLVItems
        Search_Related.ItemsSource = RelatedLVItems
        Search_Similar.ItemsSource = SimilarLVItems
        Search_Titles.ItemsSource = TitlesLVItems
        UpdateSearchTab()
    End Sub
    Public Sub UpdateSearchTab()
        Dispatcher.BeginInvoke(Async Function()
                                   With Tag
                                       If .Picture IsNot Nothing Then
                                           Search_Cover.Source = Utils.ImageSourceFromBitmap(.Picture)
                                       End If
                                       Search_Title.Text = .Titles.FirstOrDefault(Function(k) k.Type.ToLower = "main")?.Value
                                       Search_Description.Text = .Description
                                       With .Ratings.FirstOrDefault(Function(k) k.Type = AniDB.AniDBClient.RatingElement.RatingType.Parmanent)
                                           Try
                                               Search_PermaRating.Text = .Value & " By " & .Count & " User"
                                           Catch
                                               Search_PermaRating.Text = "N/A"
                                           End Try
                                       End With
                                       With .Ratings.FirstOrDefault(Function(k) k.Type = AniDB.AniDBClient.RatingElement.RatingType.Temporary)
                                           Try
                                               Search_AvgRating.Text = .Value & " By " & .Count & " User"
                                           Catch
                                               Search_AvgRating.Text = "N/A"
                                           End Try
                                       End With
                                       Search_Format.Text = "Format: " & .Type.ToString
                                       Search_EpisodeCount.Text = "Episode Count: " & .EpisodeCount
                                       Search_Status.Text = "Status: " & If(.EndDate = Date.MinValue, "Airing", "Finished")
                                       Search_StartDate.Text = "Start Date: " & .StartDate.ToShortDateString
                                       Search_EndDate.Text = "End Date: " & .EndDate.ToShortDateString
                                       Search_SeasonDate.Text = "Season: " & Utils.DateToSeason(.StartDate) & Space(1) & .StartDate.Year
                                       Search_Tags.Children.Clear()
                                       For Each animetag In .Tags
                                           Search_Tags.Children.Add(New Button With {.Content = animetag.Name, .ToolTip = animetag.Description, .Margin = New Thickness(10, 10, 0, 0)})
                                       Next
                                       Search_Characters.Children.Clear()
                                       For Each character In .Characters
                                           Search_Characters.Children.Add(New CharacterElement(character) With {.Margin = New Thickness(10, 10, 0, 0)})
                                       Next
                                       CreatorsLVItems.Clear()
                                       For Each creator In .Creators
                                           CreatorsLVItems.Add(New LVCreatorItem(creator.Value, creator.Type) With {.Tag = creator})
                                       Next
                                       TitlesLVItems.Clear()
                                       For Each AnimeTitle In .Titles
                                           TitlesLVItems.Add(New LVTitleItem(AnimeTitle.Value, AnimeTitle.Type, AnimeTitle.Lang) With {.Tag = AnimeTitle})
                                       Next
                                       RelatedLVItems.Clear()
                                       For Each related In .RelatedAnimes
                                           RelatedLVItems.Add(New LVRelatedItem(related.Value, related.Type.ToString) With {.Tag = related})
                                       Next
                                       SimilarLVItems.Clear()
                                       For Each similar In .SimilarAnimes
                                           SimilarLVItems.Add(New LVSimilarItem(similar.Value, similar.Approval.ToString) With {.Tag = similar})
                                       Next
                                   End With
                                   Dim MWnd = TryCast(Application.Current.MainWindow, MainWindow)
                                   If MWnd IsNot Nothing Then
                                       If MWnd.MainLibrary.CheckIfExists(Tag.ID) Then
                                           Search_SetAs_Watching.Visibility = Visibility.Collapsed
                                           Search_SetAs_Completed.Visibility = Visibility.Collapsed
                                           Search_SetAs_Planning.Visibility = Visibility.Collapsed
                                           Search_SetAs_Paused.Visibility = Visibility.Collapsed
                                           Search_SetAs_Dropped.Visibility = Visibility.Collapsed
                                           Search_SetAs_Edit.Visibility = Visibility.Visible
                                       Else
                                           Search_SetAs_Watching.Visibility = Visibility.Visible
                                           Search_SetAs_Completed.Visibility = Visibility.Visible
                                           Search_SetAs_Planning.Visibility = Visibility.Visible
                                           Search_SetAs_Paused.Visibility = Visibility.Visible
                                           Search_SetAs_Dropped.Visibility = Visibility.Visible
                                           Search_SetAs_Edit.Visibility = Visibility.Collapsed
                                       End If
                                   End If
                               End Function, System.Windows.Threading.DispatcherPriority.Render)
    End Sub

    Private Sub TopLeftGrid_MouseEnter(sender As Object, e As MouseEventArgs) Handles TopLeftGrid.MouseEnter
        TopLeftGrid.BeginAnimation(OpacityProperty, New Animation.DoubleAnimation(1, New Duration(TimeSpan.FromMilliseconds(100))))
    End Sub

    Private Sub TopLeftGrid_MouseLeave(sender As Object, e As MouseEventArgs) Handles TopLeftGrid.MouseLeave
        TopLeftGrid.BeginAnimation(OpacityProperty, New Animation.DoubleAnimation(0.15, New Duration(TimeSpan.FromMilliseconds(100))))
    End Sub

    Private Sub Return_BTN_Click(sender As Object, e As RoutedEventArgs) Handles Return_BTN.Click
        DialogResult = True
    End Sub
End Class

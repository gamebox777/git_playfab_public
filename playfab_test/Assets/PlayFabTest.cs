using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PlayFab;
using PlayFab.ClientModels;

//参考サイト
//https://docs.microsoft.com/ja-jp/gaming/playfab/features/social/tournaments-leaderboards/quickstart
//https://qiita.com/_y_minami/items/9143502f465ad11ff2ca

/// <summary>
/// Playfabテストコード
/// 　※雑なコードですまぬ
/// </summary>
public class PlayFabTest : MonoBehaviour
{
	public Text textGuid;
	public Text textDiceNumber;
	public Text textStatus;
	public Text textRanking;
	public static string mStrPlayfabStatus = "";
	int mDiceNumber = 0;

	static readonly string GuidKey = "Guid";

	string mGuid;
	string mUserName;	//仮：Guidから最初の6文字を切り出す

	LoginResult mLoginResult;	//ログインリザルト

	string mStrStatisticName = "LeaderBoardsTest01";

	// Title ID
	//  todo:ホントはplayfabのスクリプタブルオブジェクトから取得したいんだけど、値がいつも消えるからコードに直書き
	string mTitleIdString = "84830";	

	int mMaxEntriesCount = 100;	//ランキング取得数

	// Start is called before the first frame update
	void Start()
    {
		Login();
	}

	/// <summary>
	/// ログイン関数
	/// </summary>
	void Login()
	{
		SetPlayfabStatus("ログイン試行");
		TryLogin(OnLoginSuccess, OnLoginFailure);

		void OnLoginSuccess()
		{
			SetPlayfabStatus("ログイン成功！");
		}

		void OnLoginFailure(string report)
		{
			SetPlayfabStatus("ログイン失敗");
		}

		string str = "あなたのGUID ※ランダムGUIDの先頭6文字がランキングネーム\n" + mGuid;
		textGuid.text = str;
	}

	/// <summary>
	/// ログイン試行
	/// </summary>
	/// <param name="onSuccess"></param>
	/// <param name="onFailure"></param>
	public void TryLogin(Action onSuccess, Action<string> onFailure)
	{
		// Inspector で設定
		//if (string.IsNullOrEmpty(PlayFabSettings.TitleId))
		if (string.IsNullOrEmpty( mTitleIdString ) )
		{
			onFailure?.Invoke("PlayFabSettings.TitleIdが設定されていません");
			return;
		}

		Action<LoginResult> resultCallback = _result =>
		{
			Debug.Log("PlayFabId" + _result.PlayFabId);
			mLoginResult = _result;
			onSuccess?.Invoke();

			//ログインに成功したらユーザーネームの登録も行う
			SetUserName(mUserName);
		};
		Action<PlayFabError> errorCallback = _error =>
		{
			var report = _error.GenerateErrorReport();
			Debug.LogError(report);
			onFailure?.Invoke(report);
		};

		TryLoginDefault(resultCallback, errorCallback);
	}

	void TryLoginDefault(Action<LoginResult> resultCallback, Action<PlayFabError> errorCallback)
	{
		// GUIDランダム生成
		var guid = PlayerPrefs.GetString(GuidKey);
		if (string.IsNullOrEmpty(guid))
		{
			guid = Guid.NewGuid().ToString("D");
			PlayerPrefs.SetString(GuidKey, guid);
			PlayerPrefs.Save();
		}
		mGuid = guid;
		Debug.Log("GUID(ランダム生成)\n" + mGuid);
		mUserName = mGuid.Substring(0, 6);	//GUIDから先頭6文字を切り出す

		var request = new LoginWithCustomIDRequest
		{
			CustomId = mGuid,
			CreateAccount = true
		};
		PlayFabClientAPI.LoginWithCustomID(request, resultCallback, errorCallback);
	}

	/// <summary>
	/// ユーザーネーム登録
	/// </summary>
	/// <param name="userName"></param>
	public void SetUserName(string userName)
	{
		var request = new UpdateUserTitleDisplayNameRequest
		{
			DisplayName = userName
		};

		PlayFabClientAPI.UpdateUserTitleDisplayName(request, OnSuccess, OnError);

		void OnSuccess(UpdateUserTitleDisplayNameResult result)
		{
			SetPlayfabStatus("ユーザーネームを登録成功！" + result.DisplayName);
		}

		void OnError(PlayFabError error)
		{
			//Debug.Log($"{error.Error}");
			SetPlayfabStatus("ユーザーネーム登録失敗：" + error.Error);
		}
	}

	/// <summary>
	/// スコア登録
	/// </summary>
	/// <param name="playerScore"></param>
	public void SubmitScore(int playerScore)
	{
		PlayFabClientAPI.UpdatePlayerStatistics(new UpdatePlayerStatisticsRequest
		{
			Statistics = new List<StatisticUpdate> {
			new StatisticUpdate {
				StatisticName = mStrStatisticName,
				Value = playerScore
			}
		}
		}, result => OnStatisticsUpdated(result), FailureCallback);


		void OnStatisticsUpdated(UpdatePlayerStatisticsResult updateResult)
		{
			SetPlayfabStatus("ハイスコアを登録");	//デバッグ用関数
		}

		void FailureCallback(PlayFabError error)
		{
			string str = "API呼び出しで問題が発生しました。 デバッグ情報は次のとおりです↓";
			str += error.GenerateErrorReport();
			SetPlayfabStatus(str);  //デバッグ用関数
		}
	}

	/// <summary>
	/// ボタン：サイコロを振る
	/// </summary>
	public void ButtonDice()
	{
		mDiceNumber = UnityEngine.Random.Range(0, 999999);
		textDiceNumber.text = mDiceNumber.ToString();
	}

	/// <summary>
	/// ボタン：ハイスコア登録
	/// </summary>
	public void ButtonTouroku()
	{
		SubmitScore(mDiceNumber);
	}

	/// <summary>
	/// ボタン：ユーザーネーム
	/// </summary>
	public void ButtonSetUserName()
	{
		SetPlayfabStatus("ユーザーネームを登録したい");
		SetUserName(mUserName);
	}

	/// <summary>
	/// ボタン：ランキング取得
	/// </summary>
	public void ButtonLeaderboardGet()
	{
		LeaderboardRequester();
	}

	/// <summary>
	/// リーダーボード関連
	/// </summary>

	public void LeaderboardRequester()
	{
		//StartCoroutine("GetLeaderboard", 0f);
		SetPlayfabStatus("ランキングを表示したい");    //デバッグ用関数
		RequestLeaderboard(SetupLeaderboard);
	}

	void SetupLeaderboard(List<PlayerLeaderboardEntry> leaderboardEntries)
	{
		Debug.Log("ダミー関数");
	}

	public void RequestLeaderboard(Action<List<PlayerLeaderboardEntry>> onReceiveLeaderboard)
	{
		//var connectingView = ConnectingView.Show();

		var request = new GetLeaderboardRequest
		{
			MaxResultsCount = mMaxEntriesCount,
			StatisticName = mStrStatisticName,
		};
		PlayFabClientAPI.GetLeaderboard(
			request,
			_result => {
				SetPlayfabStatus("ランキングを取得成功");	//デバッグ用関数

				DebugLogLeaderboard(_result);
				//connectingView.Close();
				//onReceiveLeaderboard?.Invoke(_result.Leaderboard);
			},
			_error => {
				var report = _error.GenerateErrorReport();
				SetPlayfabStatus("ランキングを取失敗:" + report); //デバッグ用関数
#if false
				connectingView.Close();
				ErrorDialogView.Show("GetLeaderboard failed", report, () => {
					Request(onReceiveLeaderboard);
				}, true);
#endif
			});
	}

	void DebugLogLeaderboard(GetLeaderboardResult result)
	{
		var stringBuilder = new StringBuilder();
		foreach (var entry in result.Leaderboard)
		{
			//stringBuilder.AppendFormat(string.Format("{0}:{1}:{2}:{3}\n", entry.Position, entry.StatValue, entry.PlayFabId, entry.DisplayName));
			stringBuilder.AppendFormat(string.Format("順位：{0}・スコア：{1}・PlayFabID：{2}・表示名：{3}\n", entry.Position, entry.StatValue, entry.PlayFabId, entry.DisplayName));
		}
		Debug.Log(stringBuilder);
		SetRankingText(stringBuilder.ToString());
	}

	/// <summary>
	/// ログ表示用テキストセット
	/// </summary>
	/// <param name="str"></param>
	public void SetPlayfabStatus(string str)
	{
		mStrPlayfabStatus = str;
		Debug.Log(mStrPlayfabStatus);
		textStatus.text = mStrPlayfabStatus;
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="str"></param>
	public void SetRankingText(string str)
	{
		textRanking.text = str;
	}


}

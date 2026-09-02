using System;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.Vivox;
using Unity.Services.Authentication;

public class VivoxManager : MonoBehaviour
{
    public static VivoxManager Instance { get; private set; }

    
    public string CurrentChannel { get; private set; }


    private bool _initialized;
    private bool _loggedIn;
    

    public async Task EnsureLoggedInAsync()
    {
        if (!_initialized)
        {
            await VivoxService.Instance.InitializeAsync();
            _initialized = true;
        }

        if (!_loggedIn)
        {
            // Make sure AuthenticationService is signed in first
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }

            await VivoxService.Instance.LoginAsync();
            _loggedIn = true;
        }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Update()
    {
        //VivoxService.Instance.RunOnce();
    }

    public async Task InitializeAsync()
    {
        try
        {
            await VivoxService.Instance.InitializeAsync();
            Debug.Log("Vivox Initialized");
        }
        catch (Exception e)
        {
            Debug.LogError($"Vivox Init Failed: {e}");
        }
    }

    public async Task LoginAsync()
    {
        try
        {
            // Requires AuthenticationService.Instance to already be signed in
            await VivoxService.Instance.LoginAsync();

            _loggedIn = true;

            Debug.Log($"Vivox Login Success: {AuthenticationService.Instance.PlayerId}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Vivox Login Failed: {e}");
        }
    }

    public async Task JoinChannelAsync(string channelName)
    {
        if (!_loggedIn)
        {
            Debug.LogWarning("Login first.");
            return;
        }

        try
        {
            await VivoxService.Instance.JoinGroupChannelAsync(
                channelName,
                ChatCapability.AudioOnly
            );

            CurrentChannel = channelName;

            Debug.Log($"Joined {channelName}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Join Failed: {e}");
        }
    }

    public async Task LeaveChannelAsync()
    {
        if (string.IsNullOrEmpty(CurrentChannel))
            return;

        try
        {
            await VivoxService.Instance.LeaveChannelAsync(CurrentChannel);

            Debug.Log($"Left {CurrentChannel}");

            CurrentChannel = null;
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    public async Task LogoutAsync()
    {
        try
        {
            await LeaveChannelAsync();

            await VivoxService.Instance.LogoutAsync();

            _loggedIn = false;

            Debug.Log("Vivox Logged Out");
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    public void SetMicMuted(bool muted)
    {
        if (muted)
            VivoxService.Instance.MuteInputDevice();
        else
            VivoxService.Instance.UnmuteInputDevice();
    }

    public void SetSpeakerMuted(bool muted)
    {
        
        if (muted)
            VivoxService.Instance.MuteOutputDevice();
        else
            VivoxService.Instance.UnmuteOutputDevice();
    }

    public async Task ToggleMutePlayer(string playerId, bool mute)
    {
        if (mute)
            await VivoxService.Instance.BlockPlayerAsync(playerId);
        else
            await VivoxService.Instance.UnblockPlayerAsync(playerId);
    }
}
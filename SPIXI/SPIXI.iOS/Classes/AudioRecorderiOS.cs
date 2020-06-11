﻿using AudioToolbox;
using AVFoundation;
using Foundation;
using IXICore.Meta;
using SPIXI.VoIP;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using Xamarin.Forms;

[assembly: Dependency(typeof(AudioRecorderiOS))]

public class AudioRecorderiOS : IAudioRecorder, IAudioEncoderCallback
{
    private Action<byte[]> OnSoundDataReceived;

    private AVAudioEngine audioRecorder = null;
    private IAudioEncoder audioEncoder = null;

    bool running = false;

    List<byte[]> outputBuffers = new List<byte[]>();

    Thread recordThread = null;

    int sampleRate = SPIXI.Meta.Config.VoIP_sampleRate;
    int bitRate = SPIXI.Meta.Config.VoIP_bitRate;
    int channels = SPIXI.Meta.Config.VoIP_channels;

    public AudioRecorderiOS()
    {

    }

    public void start(string codec)
    {
        if (running)
        {
            Logging.warn("Audio recorder is already running.");
            return;
        }
        running = true;

        lock (outputBuffers)
        {
            outputBuffers.Clear();
        }

        initEncoder(codec);
        initRecorder();

        recordThread = new Thread(recordLoop);
        recordThread.Start();
    }

    private void initRecorder()
    {
        audioRecorder = new AVAudioEngine();
        NSError error = null;
        AVAudioSession.SharedInstance().SetPreferredSampleRate(sampleRate, out error);
        AVAudioFormat recording_format = new AVAudioFormat(AVAudioCommonFormat.PCMInt16, sampleRate, (uint)channels, true);
        uint buffer_size = (uint)(40 * 4 * channels * sampleRate / 1000);
        audioRecorder.InputNode.InstallTapOnBus(0, buffer_size, recording_format, onDataAvailable);
        audioRecorder.Prepare();
        audioRecorder.StartAndReturnError(out error);
    }

    private void onDataAvailable(AVAudioPcmBuffer buffer, AVAudioTime when)
    {
        AudioBuffer audioBuffer = buffer.AudioBufferList[0];
        byte[] data = new byte[audioBuffer.DataByteSize];
        Marshal.Copy(audioBuffer.Data, data, 0, audioBuffer.DataByteSize);
        
        encode(data, 0, data.Length);
    }

    private void initEncoder(string codec)
    {
        switch (codec)
        {
            case "opus":
                initOpusEncoder();
                break;

            default:
                throw new Exception("Unknown recorder codec selected " + codec);
        }
    }

    private void initOpusEncoder()
    {
        audioEncoder = new OpusEncoder(sampleRate, 24000, 1, Concentus.Enums.OpusApplication.OPUS_APPLICATION_VOIP, this);
        audioEncoder.start();
    }

    public void stop()
    {
        if (!running)
        {
            return;
        }
        running = false;

        if (audioRecorder != null)
        {
            try
            {
                audioRecorder.Stop();
            }
            catch (Exception)
            {

            }
            audioRecorder.Dispose();
            audioRecorder = null;
        }

        if (audioEncoder != null)
        {
            audioEncoder.stop();
            audioEncoder.Dispose();
            audioEncoder = null;
        }

        lock (outputBuffers)
        {
            outputBuffers.Clear();
        }
    }

    public void Dispose()
    {
        stop();
    }

    public bool isRunning()
    {
        return running;
    }

    public void setOnSoundDataReceived(Action<byte[]> on_sound_data_received)
    {
        OnSoundDataReceived = on_sound_data_received;
    }

    private void recordLoop()
    {
        while (running)
        {
            Thread.Sleep(10);

            try
            {
                sendAvailableData();
            }
            catch (Exception e)
            {
                Logging.error("Exception occured while recording audio stream: " + e);
            }
        }
        recordThread = null;
    }

    private void encode(byte[] buffer, int offset, int size)
    {
        if (!running)
        {
            return;
        }
        if (size > 0)
        {
            try
            {
                byte[] encoded_bytes = audioEncoder.encode(buffer, offset, size);
                if (encoded_bytes != null)
                {
                    onEncodedData(encoded_bytes);
                }
            }
            catch (Exception e)
            {
                Logging.error("Exception occured in encode loop: " + e);
            }
        }
    }

    private void sendAvailableData()
    {
        if (!running)
        {
            return;
        }
        byte[] data_to_send = null;
        lock (outputBuffers)
        {
            int total_size = 0;
            foreach (var buf in outputBuffers)
            {
                total_size += buf.Length;
            }

            if (total_size >= 400)
            {
                data_to_send = new byte[total_size];
                int data_written = 0;
                foreach (var buf in outputBuffers)
                {
                    Array.Copy(buf, 0, data_to_send, data_written, buf.Length);
                    data_written += buf.Length;
                }
                outputBuffers.Clear();
            }
        }
        if (data_to_send != null)
        {
            OnSoundDataReceived(data_to_send);
        }
    }

    public void onEncodedData(byte[] data)
    {
        if (!running)
        {
            return;
        }
        lock (outputBuffers)
        {
            outputBuffers.Add(data);
        }
    }
}
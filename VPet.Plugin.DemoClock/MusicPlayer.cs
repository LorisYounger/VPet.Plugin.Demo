using System;
using System.IO;
using NAudio.Wave;

namespace VPet.Plugin.DemoClock
{
    public class MusicPlayer
    {
        private IWavePlayer waveOutDevice;
        private AudioFileReader audioFileReader;
        private string currentFilePath;

        // 事件：当播放开始时触发
        public event EventHandler PlayStarted;

        // 事件：当播放暂停时触发
        public event EventHandler PlayPaused;

        // 事件：当播放结束时触发
        public event EventHandler PlayStopped;

        public MusicPlayer()
        {
            waveOutDevice = new WaveOutEvent(); // 使用 WaveOut 作为音频输出
            waveOutDevice.PlaybackStopped += WaveOutDevice_PlaybackStopped; // 监听播放结束事件
        }

        // 播放音乐
        public void Play(string filePath)
        {
            try
            {
                // 验证文件路径
                if (string.IsNullOrWhiteSpace(filePath))
                {
                    throw new ArgumentException("文件路径不能为空或空白");
                }

                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException("指定的文件未找到", filePath);
                }

                if (currentFilePath != filePath)
                {
                    Stop(); // 停止当前播放的音频

                    // 创建新的音频文件读取器
                    currentFilePath = filePath;
                    audioFileReader = new AudioFileReader(filePath);
                    waveOutDevice.Init(audioFileReader); // 初始化播放器

                    // 播放
                    waveOutDevice.Play();
                    OnPlayStarted();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"播放音频文件时发生错误: {ex.Message}");
            }
        }

        // 暂停播放
        public void Pause()
        {
            try
            {
                if (waveOutDevice.PlaybackState == PlaybackState.Playing)
                {
                    waveOutDevice.Pause();
                    OnPlayPaused();
                }
                else
                {
                    Console.WriteLine("当前没有正在播放的音频");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"暂停播放时发生错误: {ex.Message}");
            }
        }

        // 停止播放
        public void Stop()
        {
            try
            {
                if (waveOutDevice.PlaybackState != PlaybackState.Stopped)
                {
                    waveOutDevice.Stop();
                    audioFileReader?.Dispose(); // 清理音频读取器
                    audioFileReader = null; // 重置音频读取器
                    OnPlayStopped();
                }
                else
                {
                    Console.WriteLine("当前没有正在播放的音频");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"停止播放时发生错误: {ex.Message}");
            }
        }

        // 当播放开始时触发事件
        protected virtual void OnPlayStarted()
        {
            PlayStarted?.Invoke(this, EventArgs.Empty);
        }

        // 当播放暂停时触发事件
        protected virtual void OnPlayPaused()
        {
            PlayPaused?.Invoke(this, EventArgs.Empty);
        }

        // 当播放结束时触发事件
        protected virtual void OnPlayStopped()
        {
            PlayStopped?.Invoke(this, EventArgs.Empty);
        }

        // 播放结束时的处理（重置currentFilePath，并清理资源）
        private void WaveOutDevice_PlaybackStopped(object sender, StoppedEventArgs e)
        {
            try
            {
                currentFilePath = null; // 播放完成后重置currentFilePath
                audioFileReader?.Dispose(); // 清理音频读取器
                audioFileReader = null; // 重置音频读取器
                OnPlayStopped(); // 触发播放结束事件
            }
            catch (Exception ex)
            {
                Console.WriteLine($"播放结束时发生错误: {ex.Message}");
            }
        }

        // 清理资源
        public void Dispose()
        {
            try
            {
                waveOutDevice?.Dispose();
                audioFileReader?.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"清理资源时发生错误: {ex.Message}");
            }
        }
    }
}

import wave
import struct
import math
import os

def save_wav(filename, samples, sample_rate=44100):
    with wave.open(filename, 'w') as wav_file:
        wav_file.setnchannels(1)
        wav_file.setsampwidth(2)
        wav_file.setframerate(sample_rate)
        byte_data = bytearray(len(samples) * 2)
        for i, s in enumerate(samples):
            val = int(max(-1.0, min(1.0, s)) * 32767)
            struct.pack_into('<h', byte_data, i*2, val)
        wav_file.writeframesraw(byte_data)

def note_to_freq(note):
    return 440.0 * (2.0 ** (note / 12.0))

def generate_bgm():
    sample_rate = 44100
    bpm = 130
    beat_dur = 60.0 / bpm
    total_beats = 16 
    total_samples = int(sample_rate * beat_dur * total_beats)
    samples = [0.0] * total_samples
    
    bass_prog = [-24, -28, -21, -26]
    
    for i in range(total_samples):
        t = i / sample_rate
        beat = int(t / beat_dur)
        if beat >= total_beats: break
        
        chord_idx = (beat // 4) % 4
        bass_note = bass_prog[chord_idx]
        
        sixteenth = int((t % beat_dur) / (beat_dur / 4))
        bass_freq = note_to_freq(bass_note)
        b_val = 1.0 if math.sin(2 * math.pi * bass_freq * t) > 0 else -1.0
        
        t_sixteenth = (t % (beat_dur / 4))
        b_env = math.exp(-t_sixteenth * 15)
        
        arp_notes = [0, 3, 7, 12, 7, 3, -5, 0]
        arp_note = bass_note + 24 + arp_notes[(beat * 4 + sixteenth) % len(arp_notes)]
        arp_freq = note_to_freq(arp_note)
        a_val = math.asin(math.sin(2 * math.pi * arp_freq * t)) / (math.pi/2)
        a_env = math.exp(-t_sixteenth * 10)
        
        k_val = 0.0
        t_beat = t % beat_dur
        if t_beat < 0.1:
            k_freq = 150 - (t_beat * 1000)
            k_val = math.sin(2 * math.pi * max(20, k_freq) * t) * math.exp(-t_beat * 30)
            
        mixed = (b_val * b_env * 0.4) + (a_val * a_env * 0.3) + (k_val * 0.5)
        samples[i] = mixed * 0.5
        
    return samples

os.makedirs('Assets/Resources/Sounds', exist_ok=True)
save_wav('Assets/Resources/Sounds/BGM.wav', generate_bgm())
print("BGM generated")

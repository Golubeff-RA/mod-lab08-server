import pandas as pd
import matplotlib.pyplot as plt
import os

plt.rcParams.update({
    'font.size': 12,
    'lines.markersize': 8,
    'lines.linewidth': 1.5
})

df = pd.read_csv('stats.txt', sep=' ', header=1, decimal=',',
                 names=['lambda', 'mu', 'P0_exp', 'P0_theory', 
                        'Pn_exp', 'Pn_theory', 'Q_exp', 'Q_theory',
                        'A_exp', 'A_theory', 'k_exp', 'k_theory'])
df = df.sort_values('lambda') 
print(df.head())


metrics = [
    ('P0', 'Вероятность простоя'),
    ('Pn', 'Вероятность отказа'),
    ('Q', 'Относительная пропускная способность'),
    ('A', 'Абсолютная пропускная способность'),
    ('k', 'Среднее число занятых каналов')
]
i = 1
for metric, title in metrics:
    fig, ax = plt.subplots(figsize=(10, 6))
    ax.plot(
        df['lambda'], 
        df[f'{metric}_theory'], 
        color='green',
        linestyle='--',
        label='Theory'
    )

    ax.plot(
        df['lambda'], 
        df[f'{metric}_exp'], 
        color='red',
        marker='x',
        markersize=6,
        markerfacecolor='white',
        markeredgewidth=1.5,
        label='Exp'
    )

    ax.set_xlabel('request/sec')
    ax.set_ylabel(title)
    ax.set_title(f"{title}; lambda = {df['mu'].iloc[0]}", pad=15)
    ax.legend()
    ax.grid(True, alpha=0.3)
    
    filename = os.path.join('result', f'p-{i}.png')
    plt.savefig(filename, dpi=300, bbox_inches='tight')
    plt.close()
    i+=1

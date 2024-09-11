# Projeto (1 Bimestre): Processamento de Imagens

## Descrição
O foco está em três operações principais: afinamento utilizando o algoritmo Zhang-Suen, detecção de contornos e cálculo do retângulo mínimo envolvente para objetos em imagens.

## Etapas do Desenvolvimento

### 1. Algoritmo de Afinamento (Zhang-Suen)
O algoritmo de afinamento Zhang-Suen é implementado para reduzir a espessura de objetos binarizados em uma imagem, deixando-os com uma espessura de 1 pixel.

- **Entrada**: Imagem binarizada (preto e branco).
- **Saída**: Imagem afinada, com os objetos reduzidos a linhas de 1 pixel de espessura.

### 2. Algoritmo de Contorno
Este algoritmo identifica os contornos de objetos presentes em uma imagem. Ele percorre a imagem para encontrar mudanças bruscas de intensidade, o que permite delimitar os objetos.

- **Entrada**: Imagem resultante do Afinamento.
- **Saída**: Imagem apenas com os contornos.

### 3. Algoritmo do Retângulo Mínimo
Este algoritmo calcula o menor retângulo possível que envolve os objetos presentes na imagem.

- **Entrada**: Imagem resultante do algoritmo de Contorno
- **Saída**: Imagem com os retângulos minimos destacados em vermelho.

## Tecnologias Utilizadas
- C Sharp


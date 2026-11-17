import { api } from '../../infrastructure/api/apiClient.js';
import { collectPracticeSentences } from '../../domain/practice.js';
import { shuffle } from '../../shared/utils.js';

export async function loadPracticeSentences() {
  // Reload each session so practice follows the current learning library.
  const sources = ['Từ vựng', 'Ngữ pháp', 'Mẫu câu'];
  const results = await Promise.allSettled([
    api.getRandom({ count: 100 }),
    api.getGrammar(),
    api.getPatterns(),
  ]);
  const data = results.map(result => result.status === 'fulfilled' && Array.isArray(result.value) ? result.value : []);
  const entries = collectPracticeSentences(...data);
  // Give every available source a place in the session, regardless of catalog size.
  const bySource = ['vocabulary', 'grammar', 'patterns'].map(source => shuffle(entries.filter(entry => entry.source === source)));
  const sentences = [];
  while (bySource.some(pool => pool.length)) {
    bySource.forEach(pool => { if (pool.length) sentences.push(pool.pop()); });
  }
  return {
    sentences,
    failedSources: results.flatMap((result, index) => result.status === 'rejected' ? [sources[index]] : []),
  };
}
